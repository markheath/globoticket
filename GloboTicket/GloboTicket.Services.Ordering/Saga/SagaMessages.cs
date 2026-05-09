namespace GloboTicket.Services.Ordering.Saga;

// Internal saga messages. They flow between OrderSaga and its in-process
// step handlers via Wolverine + RabbitMQ but are conceptually private
// to the ordering service — no other service ever sees them. Keeping
// them out of GloboTicket.Messages stops orchestration internals from
// leaking across the inter-service contract surface.

// --- Reservation step (per line) -------------------------------------

// Saga → ReserveTicketsHandler. Emitted once per line in the order.
public record ReserveTicketsRequested(Guid OrderId, Guid EventId, int Count);

// ReserveTicketsHandler → saga. Successful reservation; saga records
// it for possible compensation later.
public record TicketsReserved(Guid OrderId, Guid EventId, int Count);

// ReserveTicketsHandler → saga. Catalog said "no" (sold out or not
// enough stock). Saga compensates any earlier reservations and fails
// the order.
public record TicketsReservationFailed(Guid OrderId, Guid EventId, int Count, string Reason);

// --- Charge step -----------------------------------------------------

// Saga → ChargeCardHandler. Total in whole units (matches Price on
// OrderLine — no decimal money type in the demo).
public record ChargeCardRequested(Guid OrderId, string CreditCardNumber, int Amount);

// ChargeCardHandler → saga. PaymentReference is the opaque txn id
// from the (mock) payment provider; this is what gets persisted on
// the Order row.
public record CardCharged(Guid OrderId, string PaymentReference);

// ChargeCardHandler → saga. Mock declines when the PAN ends in 0000;
// saga compensates all reservations and fails the order.
public record CardChargeFailed(Guid OrderId, string Reason);

// --- Persist + email checkpoints -------------------------------------

// Saga → saga. Cascaded by the saga to itself after the persist work
// completes so saga state checkpoints between persist and email — a
// crash mid-flight resumes from the right place.
public record OrderPersisted(Guid OrderId);

// Saga → saga. Same pattern: saga cascades this to itself after the
// email handler returns so saga state advances and the saga can mark
// itself complete in a fresh transaction.
public record OrderEmailSent(Guid OrderId);

// --- Compensation ----------------------------------------------------

// Saga → ReleaseTicketsHandler. Emitted once per previously successful
// reservation when a later step fails. Idempotent at the catalog: if
// a release runs twice the stock just goes higher; no harm in the demo.
public record ReleaseTicketsRequested(Guid OrderId, Guid EventId, int Count);
