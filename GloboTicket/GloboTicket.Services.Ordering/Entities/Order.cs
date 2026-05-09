namespace GloboTicket.Services.Ordering.Entities;

// Domain order — the long-lived record of a placed order. Recognisable
// to anyone who has seen an e-commerce orders table: customer snapshot,
// shipping address, lines, total, status, payment reference.
//
// Workflow / saga concerns deliberately do NOT live here. The current
// step of the order saga, the in-flight credit-card details, and the
// list of reservations made so far for compensation are all on the
// transient saga state. The status endpoint composes both.
public class Order
{
    public Guid OrderId { get; set; }

    // Customer snapshot at order time. A profile change later should
    // not alter what got delivered against this order.
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingTown { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;

    // Lines persisted into a JSON column on this row. Replaced atomically
    // with the order, never edited line-by-line, so a join table buys us
    // nothing. Prices on each line are snapshots — a later catalog price
    // change must not rewrite history.
    public List<OrderLine> Lines { get; set; } = [];

    // Order total snapshot. Denormalised but historically correct.
    public int Total { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Opaque identifier returned by the (mock) payment provider when
    // the charge succeeds. The PAN itself never reaches this table —
    // it lives transiently in saga state for the duration of the charge
    // step and is discarded with the saga. Null until the charge step
    // runs; remains null on declined orders.
    public string? PaymentReference { get; set; }

    // Populated only when Status == Failed. Free-text reason from the
    // saga, e.g. "Sold out: <event>" or "Card declined".
    public string? FailureReason { get; set; }

    public DateTimeOffset PlacedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public enum OrderStatus
{
    Pending,    // saga in progress
    Confirmed,  // tickets reserved, paid, persisted, emailed
    Failed,     // compensated; will not be fulfilled
}
