# Ordering enhancement

## Context

This branch brings the non-Dapr GloboTicket demo up to feature parity with the
sibling `globoticket-dapr` project for the order-placement flow, while staying
on Wolverine + RabbitMQ + Postgres (no Dapr). The Dapr version has a checkout
form, a live status page, an inventory concept, an email step, and a workflow
that reserves → charges → persists → emails with compensation on failure. This
project has none of that yet — `Pay` just publishes a basket id and the Payment
service logs it.

The architectural decision driving this plan is to model the workflow as a
**Wolverine Saga** persisted in Postgres (Option A from the design discussion),
rather than an inline orchestrator handler. The saga is the demo counterpart to
Dapr Workflow and is the most teachable Wolverine feature for this use case.

The existing Payment microservice gets renamed to Ordering — payment becomes
one stage inside it, not its own service. The existing
`PaymentRequestMessage` / `PaymentRequestMessageV2` handlers stay in place as
the legacy-compat / message-versioning teaching artifact; they no longer sit
on the live order path.

**Out of scope:** auth, real payment provider, basket joining the saga,
identity. Charge is mocked with the same "PAN ending in 0000 declines" rule
the Dapr version uses.

**Reference implementation:** `../globoticket-dapr` — read its
`ordering/Workflows/`, `frontend/Controllers/CheckoutController.cs`, and
`frontend/Views/Checkout/` when in doubt about UX or step semantics.

## Plan

### 1. Inventory in catalog
- [ ] Add `TicketsAvailable` (int) to `GloboTicket.Services.EventCatalog.Entities.Event`
- [ ] EF migration for the new column
- [ ] Seed sample data with mixed stock: one sold-out (0), one nearly-empty (3), one near-empty (7), rest healthy (100). Mirror the dapr `SampleData.cs` pattern.
- [ ] `EventRepository.ReserveTickets(Guid, int)` using atomic `ExecuteUpdateAsync` with `WHERE TicketsAvailable >= count`
- [ ] `EventRepository.ReleaseTickets(Guid, int)` for compensation
- [ ] `POST /api/events/{id}/reserve { count }` → 200 / 409
- [ ] `DELETE /api/events/{id}/reserve { count }` → 200

### 2. Rename Payment → Ordering
- [ ] Rename `GloboTicket.Services.Payment` project + folder + csproj + namespace → `GloboTicket.Services.Ordering`
- [ ] Rename `GloboTicket.Services.Payment.Tests` → `GloboTicket.Services.Ordering.Tests` similarly
- [ ] Update `.slnx` references
- [ ] Update AppHost: `Projects.GloboTicket_Services_Payment` → `Projects.GloboTicket_Services_Ordering`, resource name `payment` → `ordering`
- [ ] Add a comment header on `NewOrderHandler` / `NewOrderV2Handler` explaining they're a frozen legacy-compat artifact, no longer on the live path
- [ ] Verify backwards-compat tests still pass

### 3. Add orderingdb + Mailpit + Order entity
- [ ] AppHost: add `orderingdb` Postgres database
- [ ] AppHost: add Mailpit (Aspire `AddMailPit` or community integration)
- [ ] Wire ordering service to `orderingdb`, RabbitMQ, eventcatalog, mailpit
- [ ] `Order` entity in ordering service: id, customer fields, lines (json or owned collection), stage, custom-status string, success/reason, created/updated timestamps
- [ ] `OrderingDbContext` + initial migration
- [ ] Wolverine saga storage configured against the same Postgres (EF integration)

### 4. New messages in `GloboTicket.Messages`
- [ ] `SubmitOrderCommand` (full order: id, customer, lines, card)
- [ ] `OrderForCreation` / `OrderLine` / `CustomerDetails` records mirroring the dapr shapes
- [ ] Step messages: `ReserveTicketsRequested`, `TicketsReserved`, `TicketsReservationFailed`, `ChargeCardRequested`, `CardCharged`, `CardChargeFailed`, `OrderPersisted`, `OrderEmailSent`, `ReleaseTicketsRequested`
- [ ] Existing `PaymentRequestMessage` / V2 stay untouched

### 5. Saga + step handlers in ordering service
- [ ] `OrderSaga : Saga` with `Id` = OrderId, fields for stage, reserved-so-far list, customer/card info
- [ ] `Start(SubmitOrderCommand)` → write Order row (Pending), cascade `ReserveTicketsRequested` for first line
- [ ] `Handle(TicketsReserved)` → cascade next line's reserve, or move to `ChargeCardRequested`
- [ ] `Handle(TicketsReservationFailed)` → cascade `ReleaseTicketsRequested` for everything reserved so far, mark Order failed
- [ ] `Handle(CardCharged)` → persist Order final state, cascade `OrderPersisted`
- [ ] `Handle(CardChargeFailed)` → cascade `ReleaseTicketsRequested` for all, mark Order failed
- [ ] `Handle(OrderPersisted)` → cascade `OrderEmailSent` after sending email
- [ ] Step handlers as separate classes: `ReserveTicketsHandler` (HTTP to catalog), `ChargeCardHandler` (in-process mock; PAN ending 0000 → fail), `ReleaseTicketsHandler` (HTTP DELETE to catalog), `SendEmailHandler` (SMTP to Mailpit)
- [ ] Each step updates Order row stage / custom-status string using Wolverine's transactional outbox so DB write + outgoing message commit together

### 6. Status endpoint
- [ ] `GET /order/{id}/status` in ordering service reading the Order row
- [ ] Response shape matches what the dapr `Order.cshtml` polling JS expects: `{ runtimeStatus, customStatus, output: { success, reason } }` so the JS port is verbatim
- [ ] Custom-status strings match the dapr ones: `"Reserving tickets"`, `"Authorizing payment"`, `"Persisting order"`, `"Sending confirmation email"`, `"Releasing reservations"`

### 7. Frontend (`GloboTicket.Client`)
- [ ] `CheckoutController` with `Index` (GET form, prefilled), `Purchase` (POST → publishes `SubmitOrderCommand` via `IMessageBus`, redirects to `Order/{id}`), `Order/{id}` (status page), `OrderStatus/{id}` (JSON pass-through to ordering)
- [ ] HttpClient registered against `https+http://ordering`
- [ ] `CheckoutViewModel` with name, email, address, town, postcode, card, expiry
- [ ] `Views/Checkout/Index.cshtml` ported from dapr (form + card-preset dropdown including the "ends in 0000 will be declined" option)
- [ ] `Views/Checkout/Order.cshtml` ported from dapr (workflow steps + polling JS)
- [ ] CSS for `.workflow-step`, `.workflow-result`, `.workflow-compensation` ported from dapr
- [ ] `ShoppingBasketController.Pay` → redirects to `Checkout/Index` instead of publishing V2 message directly. Delete `Thanks.cshtml` (or repurpose).
- [ ] Basket cleared after successful submit (call basket service from `Purchase`)

### 8. Tests
- [ ] Existing two backwards-compat tests stay green after rename
- [ ] Saga happy-path test using `TrackActivity().ExecuteAndWaitAsync` driving `SubmitOrderCommand` end-to-end
- [ ] Decline-path test (PAN ends 0000) asserts compensation `ReleaseTicketsRequested` messages emitted for each reserved line
- [ ] Sold-out-path test asserts compensation for already-reserved lines when a later line fails reservation

## Session log

### 2026-05-09 — planning
- Drafted plan and `docs/ordering-enhancement.md`. Created branch `ordering-enhancement`. No merges to `main` until all 8 steps are done.
- Architectural decision: Wolverine Saga (Option A) over inline orchestrator (B) or hybrid (C). Saga is the most teachable Wolverine feature and the cleanest counterpart to Dapr Workflow.
- Legacy-compat decision: keep `PaymentRequestMessage` / V2 + their handlers as a frozen demo of the message-versioning lesson. They're no longer on the live order path; the new path uses `SubmitOrderCommand`.
- Status-endpoint shape is deliberately copied from the dapr response so the polling JS in `Order.cshtml` ports verbatim.
