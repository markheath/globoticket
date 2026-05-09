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

- [x] Add `TicketsAvailable` (int) to `GloboTicket.Services.EventCatalog.Entities.Event`
- [x] EF migration for the new column
- [x] Seed sample data with mixed stock: one sold-out (0), one nearly-empty (3), one near-empty (7), rest healthy (100). Mirror the dapr `SampleData.cs` pattern.
- [x] `EventRepository.ReserveTickets(Guid, int)` using atomic `ExecuteUpdateAsync` with `WHERE TicketsAvailable >= count`
- [x] `EventRepository.ReleaseTickets(Guid, int)` for compensation
- [x] `POST /api/events/{id}/reserve { count }` → 200 / 409
- [x] `DELETE /api/events/{id}/reserve { count }` → 200

### 2. Rename Payment → Ordering

- [x] Rename `GloboTicket.Services.Payment` project + folder + csproj + namespace → `GloboTicket.Services.Ordering`
- [x] Rename `GloboTicket.Services.Payment.Tests` → `GloboTicket.Services.Ordering.Tests` similarly
- [x] Update `.slnx` references
- [x] Update AppHost: `Projects.GloboTicket_Services_Payment` → `Projects.GloboTicket_Services_Ordering`, resource name `payment` → `ordering`
- [x] Add a comment header on `NewOrderHandler` / `NewOrderV2Handler` explaining they're a frozen legacy-compat artifact, no longer on the live path
- [x] Verify backwards-compat tests still pass

### 3. Add orderingdb + Mailpit + Order entity

- [x] AppHost: add `orderingdb` Postgres database
- [x] AppHost: add Mailpit (Aspire `AddMailPit` or community integration)
- [x] Wire ordering service to `orderingdb`, RabbitMQ, eventcatalog, mailpit
- [x] `Order` domain entity (NOT a workflow-state record): `OrderId`, customer + shipping snapshot, lines (json column), `Total`, `Status` (Pending/Confirmed/Failed), `PaymentReference` (opaque, set by charge step — NO PAN data), `FailureReason`, `PlacedAt`, `CompletedAt`. Saga concerns deliberately NOT here.
- [x] `OrderingDbContext` + initial migration
- [x] Wolverine saga storage configured against the same Postgres (EF integration)

### 4. New messages in `GloboTicket.Messages`

- [ ] `SubmitOrderCommand` (full order: id, customer, lines, card)
- [ ] `OrderForCreation` / `OrderLine` / `CustomerDetails` records mirroring the dapr shapes
- [ ] Step messages: `ReserveTicketsRequested`, `TicketsReserved`, `TicketsReservationFailed`, `ChargeCardRequested`, `CardCharged`, `CardChargeFailed`, `OrderPersisted`, `OrderEmailSent`, `ReleaseTicketsRequested`
- [ ] Existing `PaymentRequestMessage` / V2 stay untouched

### 5. Saga + step handlers in ordering service

- [ ] `OrderSaga : Saga` with `Id` = OrderId. Saga state holds the **transient** workflow concerns: `CurrentStage` (enum: ReservingTickets / AuthorizingPayment / PersistingOrder / SendingEmail / ReleasingReservations), `CreditCardNumber` + `CreditCardExpiry` (held for charge step only, never persisted to Order), `ReservationsMade` (for compensation).
- [ ] `Start(SubmitOrderCommand)` → write Order row (`Status = Pending`, `Total` snapshotted from lines), cascade `ReserveTicketsRequested` for first line
- [ ] `Handle(TicketsReserved)` → record reservation in saga, cascade next line's reserve, or move to `ChargeCardRequested`
- [ ] `Handle(TicketsReservationFailed)` → cascade `ReleaseTicketsRequested` for everything reserved so far, set `Order.Status = Failed` + `FailureReason`
- [ ] `Handle(CardCharged)` → set `Order.PaymentReference = txn_<guid>`, cascade `OrderPersisted`
- [ ] `Handle(CardChargeFailed)` → cascade `ReleaseTicketsRequested` for all, set `Order.Status = Failed` + `FailureReason = "Card declined"`
- [ ] `Handle(OrderPersisted)` → set `Order.Status = Confirmed` + `CompletedAt`, send email, cascade `OrderEmailSent`
- [ ] Step handlers as separate classes: `ReserveTicketsHandler` (HTTP to catalog), `ChargeCardHandler` (in-process mock; PAN ending 0000 → fail; success returns fake `txn_<guid>` reference), `ReleaseTicketsHandler` (HTTP DELETE to catalog), `SendEmailHandler` (SMTP to Mailpit)
- [ ] Each step updates Order row + saga `CurrentStage` using Wolverine's transactional outbox so DB write + outgoing message commit together

### 6. Status endpoint

- [ ] `GET /order/{id}/status` in ordering service. Composes Order row + saga state.
- [ ] Response shape (domain-flavoured, NOT mimicking the dapr workflow API):

  ```json
  {
    "status": "Pending|Confirmed|Failed",
    "currentStage": "ReservingTickets|AuthorizingPayment|PersistingOrder|SendingEmail|ReleasingReservations|null",
    "failureReason": "string|null",
    "placedAt": "...",
    "completedAt": "...|null"
  }
  ```

  When `status` is terminal, `currentStage` is `null`. While `Pending`, the endpoint reads saga state to populate `currentStage`.
- [ ] 404 when Order row not found.

### 7. Frontend (`GloboTicket.Client`)

- [ ] `CheckoutController` with `Index` (GET form, prefilled), `Purchase` (POST → publishes `SubmitOrderCommand` via `IMessageBus`, redirects to `Order/{id}`), `Order/{id}` (status page), `OrderStatus/{id}` (JSON pass-through to ordering)
- [ ] HttpClient registered against `https+http://ordering`
- [ ] `CheckoutViewModel` with name, email, address, town, postcode, card, expiry
- [ ] `Views/Checkout/Index.cshtml` ported from dapr (form + card-preset dropdown including the "ends in 0000 will be declined" option)
- [ ] `Views/Checkout/Order.cshtml` ported from dapr (workflow steps + polling JS). The polling JS needs adapting to the new response shape: read `status` (not `runtimeStatus`), `currentStage` enum (not `customStatus` strings), `failureReason` (not `output.reason`). The step indices and the `failedStepFromReason` heuristic stay the same.
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
- ~~Status-endpoint shape is deliberately copied from the dapr response so the polling JS in `Order.cshtml` ports verbatim.~~ Reversed during step 3 — see step-3 log entry. The endpoint now uses domain-flavoured fields and the JS gets minor adaptation.

### 2026-05-09 — step 3 done

- Added `orderingdb` Postgres + Mailpit (CommunityToolkit `AddMailPit`, ports pinned 8025/1025) to AppHost. Ordering service references rabbit + orderingdb + eventcatalog + mailpit.
- Wolverine 5.37.2 → 5.38.0 across the family + added `WolverineFx.EntityFrameworkCore` and `WolverineFx.Postgresql`. EF Core packages bumped 10.0.0 → 10.0.2 to satisfy a transitive constraint from `WolverineFx.EntityFrameworkCore`. The bump also incidentally pulled fixes for a `Workspaces.Common 4.14 ≠ 5.0` constraint mismatch and a `System.Security.Cryptography.Xml 9.0.0` advisory that surfaced under `TreatWarningsAsErrors=true` once the new packages were referenced. `Npgsql.EntityFrameworkCore.PostgreSQL` stays at 10.0.1 — 10.0.2 isn't published yet.
- Ordering kept as a Worker host. Web SDK conversion deferred to step 6 when the status endpoint actually needs HTTP.
- `IDesignTimeDbContextFactory<OrderingDbContext>` added so `dotnet ef` can scaffold without standing up the full host. Without it, EF design-time fails because `UseRabbitMq(new Uri(...))` blows up when the rabbit URI is null at design-time.
- Wolverine envelope tables did NOT appear in the EF migration despite calling `MapWolverineEnvelopeStorage()`. Hypothesis: Wolverine's Postgres integration manages those tables itself at runtime, separate from EF migrations. Will exercise the outbox in step 5 and revisit if it fails — may need an explicit `opts.PersistMessagesWithPostgresql(...)` call.
- **Order entity redesign during this step (in response to review):** the first draft had `RuntimeStatus` / `CustomStatus` / `Success` / `FailureReason as output` / `CreditCardLast4` — workflow-engine vocabulary masquerading as domain fields. Replaced with a domain entity: `Status` (Pending / Confirmed / Failed), `PaymentReference` (opaque, set by charge step; PAN never persisted), `FailureReason`, `PlacedAt` / `CompletedAt`, plus customer + shipping snapshot, `Lines` (json), `Total` (snapshot). Workflow concerns — current stage, transient PAN/expiry for the charge step, reservations-made-so-far for compensation — move to the saga state in step 5. Status endpoint composes both. The migration was thrown away and regenerated cleanly.

### 2026-05-09 — step 2 done

- Renamed via `git mv` so history follows. Folder + csproj + namespace + UserSecretsId + launchSettings profile + `.slnx` paths + AppHost `csproj` ref + AppHost `cs` resource name all renamed in one commit.
- `PaymentRequestMessage` / `PaymentRequestMessageV2` class names + filename + `PaymentBackwardsCompatibilityTests` class name **kept** — they're the wire format / teaching artifact, renaming defeats the lesson. Only the namespace they live in changed.
- Added a `LEGACY-COMPAT ARTIFACT — frozen in place on purpose` header to `NewOrderHandler.cs` so the next reader understands these aren't the live order path. The two compat tests are the contract guard.
- Stale "Payment service" comments in the Web project (`Client/Program.cs`, `ShoppingBasketController.cs`) updated to "Ordering service" since the recipient really did rename.
- Solution builds clean. Both backwards-compat tests still green.

### 2026-05-09 — step 1 done

- Inventory lives on `Event`, not on `Ticket`. The basket and order shapes already use `EventId` + `TicketCount` only (no tier id), so per-tier stock would force basket changes that aren't in scope. Mirrors the dapr inventory model.
- Reservation endpoints went on a new un-versioned `ReservationsController` rather than wedging command endpoints onto the V1/V2 read-DTO controllers. Routes: `POST /api/events/{eventId}/reserve` and `DELETE /api/events/{eventId}/reserve`, both taking `{ count }`.
- Stock distribution mirrors dapr exactly so the saga demos behave identically: Nick Sailor 0 (sold out), John Egbert 3, Lighthouse 7, rest 100.
- Solution builds clean, two existing catalog integration tests still pass. Migration `20260509122748_AddTicketsAvailable` adds the column with `defaultValue: 0` plus 8 `UpdateData` rows for the seeds.
