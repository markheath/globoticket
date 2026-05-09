namespace GloboTicket.Messages.Ordering;

// Inter-service contract — invoked by the Web project, handled by
// SubmitOrderHandler in the Ordering service which runs the four-step
// flow inline (reserve → charge → persist → email) and returns an
// OrderResult to the caller. Wolverine's request/response pattern
// (IMessageBus.InvokeAsync) carries the response back over RabbitMQ.
//
// The credit card number rides inside the message body. In a real
// system the frontend would tokenise via the payment provider before
// submit so the PAN never reaches our infrastructure; the demo skips
// that and uses a mock charge step inside SubmitOrderHandler. The PAN
// is never persisted to the Order row.
public record SubmitOrderCommand(
    Guid OrderId,
    CustomerDetails Customer,
    IReadOnlyList<OrderLine> Lines,
    string CreditCardNumber,
    string CreditCardExpiry);

// Reply to SubmitOrderCommand. Confirmed orders carry no failure reason;
// failed orders carry a human-readable reason ("Sold out: <event>",
// "Card declined") that the frontend renders directly.
public record OrderResult(bool Confirmed, string? FailureReason);

public record CustomerDetails(
    string Name,
    string Email,
    string Address,
    string Town,
    string PostalCode);

// Wire-format order line. Snapshot at submit time — prices and event
// metadata are captured here so a later catalog change doesn't
// retroactively rewrite an in-flight order.
public record OrderLine(
    Guid EventId,
    string EventName,
    string ArtistName,
    int TicketCount,
    int Price);
