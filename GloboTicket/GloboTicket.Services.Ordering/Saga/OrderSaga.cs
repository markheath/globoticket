using GloboTicket.Messages.Ordering;
using GloboTicket.Services.Ordering.DbContexts;
using GloboTicket.Services.Ordering.Entities;
using EntityOrderLine = GloboTicket.Services.Ordering.Entities.OrderLine;
using MessageOrderLine = GloboTicket.Messages.Ordering.OrderLine;

namespace GloboTicket.Services.Ordering.Saga;

// Orchestration saga for the four-step order flow:
//
//   1. Reserve stock      (per line, against the catalog over HTTP)
//   2. Take payment       (mock — declined when PAN ends in 0000)
//   3. Save the order     (this saga's own EF write inside Handle(OrderPersisted))
//   4. Send confirmation  (SendEmailHandler over SMTP to Mailpit)
//
// Saga state itself is JSON-serialised by Wolverine's Postgres
// persistence; it carries the in-flight orchestration data — line list,
// reserve-loop pointer, reservations made for compensation, transient
// card details. Wolverine correlates incoming messages to a saga
// instance by matching the message's OrderId field against this saga's
// Id field (`{SagaName}Id` convention).
//
// The saga does NOT carry the "current stage" — that lives on a
// separate OrderProcessingState row in OrderingDbContext, written from
// each saga handler. Wolverine has no public read API for saga state,
// and putting "what step is running" on the Order entity would leak
// workflow internals into the domain table. The OrderProcessingState
// read model is the bridge.
//
// Cascading: handlers return either a single message or IEnumerable<object>
// to publish; Wolverine's outbox commits those + saga-state + EF writes
// in one transaction.
public class OrderSaga : Wolverine.Saga
{
    public Guid Id { get; set; }

    // Snapshot of the line collection — used for the per-line reserve
    // loop, total computation, and as the source of EventName when we
    // want to mention which event sold out in a failure reason.
    public List<MessageOrderLine> Lines { get; set; } = [];

    // Pointer into Lines for the per-line reserve loop. Each successful
    // TicketsReserved increments this and either advances to the next
    // line or moves to the charge step.
    public int NextLineIndex { get; set; }

    // Reservations made so far. Drives the compensation path: on any
    // failure further down the pipeline, one ReleaseTicketsRequested
    // is emitted per entry here.
    public List<ReservedLine> ReservationsMade { get; set; } = [];

    public int Total { get; set; }

    // Held only for the duration of the charge step. Cleared as soon
    // as charge resolves (success or failure) so they don't sit in
    // saga storage longer than needed. The Order row never sees these.
    public string CreditCardNumber { get; set; } = string.Empty;
    public string CreditCardExpiry { get; set; } = string.Empty;

    // Entry point. Wolverine creates the saga instance from the returned
    // tuple and persists it before the cascaded ReserveTicketsRequested
    // is dispatched.
    public static (OrderSaga, ReserveTicketsRequested) Start(
        SubmitOrderCommand cmd,
        OrderingDbContext db)
    {
        var total = cmd.Lines.Sum(l => l.Price * l.TicketCount);

        db.Orders.Add(new Order
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
            Total = total,
            Status = OrderStatus.Pending,
            PlacedAt = DateTimeOffset.UtcNow,
        });

        SetStage(db, cmd.OrderId, OrderProcessingStage.ReservingTickets);

        var saga = new OrderSaga
        {
            Id = cmd.OrderId,
            Lines = [.. cmd.Lines],
            NextLineIndex = 0,
            Total = total,
            CreditCardNumber = cmd.CreditCardNumber,
            CreditCardExpiry = cmd.CreditCardExpiry,
        };

        var first = cmd.Lines[0];
        return (saga, new ReserveTicketsRequested(cmd.OrderId, first.EventId, first.TicketCount));
    }

    // After a successful reservation: advance to the next line, or move
    // on to the charge step if all lines are done.
    public object Handle(TicketsReserved evt, OrderingDbContext db)
    {
        ReservationsMade.Add(new ReservedLine(evt.EventId, evt.Count));
        NextLineIndex++;

        if (NextLineIndex < Lines.Count)
        {
            var next = Lines[NextLineIndex];
            return new ReserveTicketsRequested(Id, next.EventId, next.TicketCount);
        }

        SetStage(db, Id, OrderProcessingStage.AuthorizingPayment);
        return new ChargeCardRequested(Id, CreditCardNumber, Total);
    }

    // Reservation failed (sold out or stock too low). Compensate any
    // earlier successful reservations and fail the order.
    public IEnumerable<object> Handle(TicketsReservationFailed evt, OrderingDbContext db)
    {
        SetStage(db, Id, OrderProcessingStage.ReleasingReservations);

        var failedLine = Lines.FirstOrDefault(l => l.EventId == evt.EventId);
        var reason = failedLine is not null
            ? $"Sold out: {failedLine.EventName}"
            : evt.Reason;

        FailOrder(db, reason);

        var releases = ReservationsMade
            .Select(r => (object)new ReleaseTicketsRequested(Id, r.EventId, r.Count))
            .ToList();

        MarkCompleted();
        return releases;
    }

    // Charge succeeded. Stash the payment reference on the Order, drop
    // the PAN from saga state, and self-cascade to the persist
    // checkpoint. The actual Status flip to Confirmed happens in the
    // next handler so PersistingOrder is observable as a discrete stage.
    public OrderPersisted Handle(CardCharged evt, OrderingDbContext db)
    {
        var order = db.Orders.Find(Id)!;
        order.PaymentReference = evt.PaymentReference;

        ClearCardDetails();
        SetStage(db, Id, OrderProcessingStage.PersistingOrder);
        return new OrderPersisted(Id);
    }

    // Charge declined. Same compensation path as a reservation failure.
    public IEnumerable<object> Handle(CardChargeFailed evt, OrderingDbContext db)
    {
        SetStage(db, Id, OrderProcessingStage.ReleasingReservations);
        ClearCardDetails();
        FailOrder(db, evt.Reason);

        var releases = ReservationsMade
            .Select(r => (object)new ReleaseTicketsRequested(Id, r.EventId, r.Count))
            .ToList();

        MarkCompleted();
        return releases;
    }

    // The persist step. Order row is flipped to Confirmed here so the
    // status endpoint reports a confirmed order before we attempt the
    // email — the email is best-effort delivery on top of an already
    // committed sale.
    public SendOrderEmailRequested Handle(OrderPersisted evt, OrderingDbContext db)
    {
        var order = db.Orders.Find(Id)!;
        order.Status = OrderStatus.Confirmed;
        order.CompletedAt = DateTimeOffset.UtcNow;

        SetStage(db, Id, OrderProcessingStage.SendingEmail);
        return new SendOrderEmailRequested(Id);
    }

    // Email sent. Saga's job is done. OrderProcessingState is left in
    // place — the status endpoint ignores it for terminal Orders.
    public void Handle(OrderEmailSent evt) => MarkCompleted();

    private void FailOrder(OrderingDbContext db, string reason)
    {
        var order = db.Orders.Find(Id)!;
        order.Status = OrderStatus.Failed;
        order.FailureReason = reason;
        order.CompletedAt = DateTimeOffset.UtcNow;
    }

    private void ClearCardDetails()
    {
        CreditCardNumber = string.Empty;
        CreditCardExpiry = string.Empty;
    }

    // Upserts the OrderProcessingState row. EF tracks the row created
    // in Start as Added on the first call and as Modified on subsequent
    // calls, all flushed in the same transaction as the saga's other
    // writes via UseEntityFrameworkCoreTransactions().
    private static void SetStage(OrderingDbContext db, Guid orderId, OrderProcessingStage stage)
    {
        var existing = db.OrderProcessingStates.Local.FirstOrDefault(s => s.OrderId == orderId)
            ?? db.OrderProcessingStates.Find(orderId);
        if (existing is null)
        {
            db.OrderProcessingStates.Add(new OrderProcessingState
            {
                OrderId = orderId,
                CurrentStage = stage,
                LastUpdatedAt = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            existing.CurrentStage = stage;
            existing.LastUpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

public record ReservedLine(Guid EventId, int Count);
