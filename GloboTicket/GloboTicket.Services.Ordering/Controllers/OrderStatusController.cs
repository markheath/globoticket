using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Ordering.Controllers;

// Read-only order endpoint for the frontend's result page. The order
// is always in a terminal state (Confirmed or Failed) by the time the
// frontend reads it — SubmitOrderCommand is invoked as a request/
// response and only returns once the order flow has resolved.
[ApiController]
[Route("order/{orderId:guid}")]
public class OrderStatusController : ControllerBase
{
    private readonly OrderingDbContext db;

    public OrderStatusController(OrderingDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid orderId)
    {
        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            status = order.Status.ToString(),
            failureReason = order.FailureReason,
            placedAt = order.PlacedAt,
            completedAt = order.CompletedAt,
        });
    }
}
