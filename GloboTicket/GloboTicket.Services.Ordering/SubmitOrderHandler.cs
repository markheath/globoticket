using GloboTicket.Messages.Ordering;
using GloboTicket.Services.Ordering.DbContexts;
using GloboTicket.Services.Ordering.Entities;
using EntityOrderLine = GloboTicket.Services.Ordering.Entities.OrderLine;

namespace GloboTicket.Services.Ordering;

// Runs the four-step order flow inline:
//
//   1. Insert Order row in Pending status.
//   2. Reserve stock per line against the catalog. On any failure,
//      release whatever was reserved so far and short-circuit to Failed.
//   3. Mock-charge the card (PAN ending 0000 declines). On failure,
//      release all reservations and short-circuit to Failed.
//   4. Flip the Order to Confirmed and persist the payment reference.
//   5. Best-effort send the confirmation email. An SMTP failure here
//      does NOT roll back the order — by this point the customer has
//      paid and the tickets are reserved; missing the email is a
//      customer service issue, not a transactional one.
//
// Compensation is the explicit `foreach (...) Release(...)` block at
// each failure point.
//
// A process crash mid-flow leaves orphaned state: a crash after step 2
// leaves stock reserved against no order; a crash after step 3 leaves
// a paid-but-not-confirmed order. Acceptable for a demo; production
// systems with this requirement would reach for a workflow engine
// (Dapr Workflow, Temporal, Durable Functions) for crash recovery.
public class SubmitOrderHandler
{
    public async Task<OrderResult> Handle(
        SubmitOrderCommand cmd,
        OrderingDbContext db,
        ICatalogReservationsClient catalog,
        EmailSender email,
        ILogger<SubmitOrderHandler> logger,
        CancellationToken ct)
    {
        var subtotal = cmd.Lines.Sum(l => l.Price * l.TicketCount);
        // Clamp so a discount can never produce a negative charge.
        var discount = Math.Clamp(cmd.DiscountAmount, 0, subtotal);
        var total = subtotal - discount;

        var order = new Order
        {
            OrderId = cmd.OrderId,
            CustomerName = cmd.Customer.Name,
            CustomerEmail = cmd.Customer.Email,
            ShippingAddress = cmd.Customer.Address,
            ShippingTown = cmd.Customer.Town,
            ShippingPostalCode = cmd.Customer.PostalCode,
            Lines = [.. cmd.Lines.Select(l => new EntityOrderLine
            {
                EventId = l.EventId,
                EventName = l.EventName,
                ArtistName = l.ArtistName,
                TicketCount = l.TicketCount,
                Price = l.Price,
            })],
            DiscountCode = cmd.DiscountCode,
            DiscountAmount = discount,
            Total = total,
            Status = OrderStatus.Pending,
            PlacedAt = DateTimeOffset.UtcNow,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        // Step 1 — reserve stock per line. Each Reserve is an atomic
        // ExecuteUpdate at the catalog; first refusal short-circuits.
        var reservations = new List<(Guid eventId, int count)>();
        foreach (var line in cmd.Lines)
        {
            var ok = await catalog.Reserve(line.EventId, line.TicketCount, ct);
            if (!ok)
            {
                logger.LogWarning("Reservation refused for {EventId} ({Count} tickets) on order {OrderId}",
                    line.EventId, line.TicketCount, cmd.OrderId);
                await ReleaseAll(reservations, catalog, ct);
                return await Fail(order, db, $"Sold out: {line.EventName}", ct);
            }
            reservations.Add((line.EventId, line.TicketCount));
        }

        // Step 2 — mock card charge. Deterministic decline rule: PAN
        // ending in "0000" fails. Successful charges produce a fake
        // txn_<guid> reference that gets stamped on the Order row.
        var charge = ChargeCard(cmd.CreditCardNumber, total, cmd.OrderId, logger);
        if (!charge.Success)
        {
            await ReleaseAll(reservations, catalog, ct);
            return await Fail(order, db, "Card declined", ct);
        }

        // Step 3 — confirm + persist. Order moves out of Pending here.
        order.PaymentReference = charge.Reference;
        order.Status = OrderStatus.Confirmed;
        order.CompletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        // Step 4 — confirmation email. Best-effort: a failed send does
        // not undo the confirmed order. Logged and swallowed.
        try
        {
            await email.SendOrderConfirmation(order, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Order {OrderId} confirmed but confirmation email failed",
                order.OrderId);
        }

        return new OrderResult(Confirmed: true, FailureReason: null);
    }

    private static async Task<OrderResult> Fail(
        Order order, OrderingDbContext db, string reason, CancellationToken ct)
    {
        order.Status = OrderStatus.Failed;
        order.FailureReason = reason;
        order.CompletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return new OrderResult(Confirmed: false, FailureReason: reason);
    }

    private static async Task ReleaseAll(
        List<(Guid eventId, int count)> reservations,
        ICatalogReservationsClient catalog,
        CancellationToken ct)
    {
        foreach (var (eventId, count) in reservations)
        {
            await catalog.Release(eventId, count, ct);
        }
    }

    // Mock charge step. Same rule as the dapr demo: any PAN whose
    // trimmed digits end in "0000" is declined. Otherwise approved
    // with a synthetic txn reference.
    private static (bool Success, string Reference) ChargeCard(
        string pan, int amount, Guid orderId, ILogger logger)
    {
        var trimmed = (pan ?? string.Empty).Trim();
        if (trimmed.EndsWith("0000"))
        {
            logger.LogWarning("Mock charge declined for order {OrderId} amount {Amount}",
                orderId, amount);
            return (false, string.Empty);
        }
        var reference = $"txn_{Guid.NewGuid():N}";
        logger.LogInformation("Mock charge approved for order {OrderId} amount {Amount} as {Ref}",
            orderId, amount, reference);
        return (true, reference);
    }
}
