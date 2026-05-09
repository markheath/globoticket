namespace GloboTicket.Messages.Ordering;

// Inter-service contract — published by the Web project, handled by
// the order saga in the Ordering service. Saga step / compensation
// messages are NOT here; they live inside the Ordering project under
// GloboTicket.Services.Ordering.Saga and never cross the bus to any
// other service.
//
// The credit card number rides inside the message body. In a real
// system the frontend would tokenise via the payment provider before
// submit so the PAN never reaches our infrastructure; the demo skips
// that and uses a mock charge step downstream. The saga holds the PAN
// transiently in saga state for the duration of the charge step and
// never persists it to the Order row.
public record SubmitOrderCommand(
    Guid OrderId,
    CustomerDetails Customer,
    IReadOnlyList<OrderLine> Lines,
    string CreditCardNumber,
    string CreditCardExpiry);

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
