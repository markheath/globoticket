using GloboTicket.Services.Ordering.DbContexts;
using GloboTicket.Services.Ordering.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Ordering.Controllers;

// Polled by the frontend's order status page. Composes the durable
// Order row with the saga's transient progress (read from
// OrderProcessingState) so the UI can render which of the four
// pipeline stages is currently active.
//
// Response shape is deliberately domain-flavoured (status, currentStage,
// failureReason) rather than mirroring a workflow-engine API; the
// frontend's polling JS adapts to this shape.
[ApiController]
[Route("order/{orderId:guid}/status")]
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

        // Saga progress is only relevant while the order is still
        // running. For terminal orders the OrderProcessingState row
        // may still exist but its "current stage" is meaningless —
        // the order is already Confirmed or Failed.
        OrderProcessingStage? currentStage = null;
        if (order.Status == OrderStatus.Pending)
        {
            var state = await db.OrderProcessingStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
            currentStage = state?.CurrentStage;
        }

        return Ok(new
        {
            status = order.Status.ToString(),
            currentStage = currentStage?.ToString(),
            failureReason = order.FailureReason,
            placedAt = order.PlacedAt,
            completedAt = order.CompletedAt,
        });
    }
}
