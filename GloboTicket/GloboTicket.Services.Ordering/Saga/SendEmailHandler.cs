using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Ordering.Saga;

// Step 4 — send the order-confirmation email. Reads the persisted
// Order row directly rather than relying on saga state because by the
// time this handler runs the Order is already Confirmed in the DB.
//
// Failure here doesn't compensate — the order has already been sold.
// Wolverine's normal retry behaviour will keep attempting the SMTP
// send; if it eventually gives up the order remains Confirmed but the
// customer just doesn't get an email.
public class SendEmailHandler
{
    public async Task<OrderEmailSent> Handle(
        SendOrderEmailRequested cmd,
        OrderingDbContext db,
        EmailSender sender,
        CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .FirstAsync(o => o.OrderId == cmd.OrderId, ct);

        await sender.SendOrderConfirmation(order, ct);
        return new OrderEmailSent(cmd.OrderId);
    }
}
