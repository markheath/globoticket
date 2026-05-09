namespace GloboTicket.Services.Ordering.Entities;

// Read model for the order saga's progress. The saga writes this row in
// the same DbContext as the Order itself so updates commit atomically;
// the status endpoint reads it for orders still in Pending. Keeping it
// in its own table rather than columns on Order means the Order entity
// stays as a clean domain record — workflow internals don't leak into
// the e-commerce table.
//
// Not deleted when the saga completes; the row is harmless either way
// because the status endpoint only reads it while Order.Status is
// Pending.
public class OrderProcessingState
{
    public Guid OrderId { get; set; }
    public OrderProcessingStage CurrentStage { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
}

public enum OrderProcessingStage
{
    ReservingTickets,
    AuthorizingPayment,
    PersistingOrder,
    SendingEmail,
    ReleasingReservations,
}
