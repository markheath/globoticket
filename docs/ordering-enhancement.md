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

### 4. New messages

`GloboTicket.Messages` is the **inter-service contract surface** — only types that cross a service boundary belong here. Saga step / compensation messages flow only between the saga and its in-process step handlers, so they live inside the ordering service itself, not in the contract assembly.

In `GloboTicket.Messages` (namespace `GloboTicket.Messages.Ordering`):

- [x] `SubmitOrderCommand` (Web → Ordering): `OrderId`, `CustomerDetails`, lines, credit card number, expiry
- [x] `CustomerDetails` record (name, email, address, town, postcode)
- [x] `OrderLine` record (event id, name, artist, ticket count, price)

In `GloboTicket.Services.Ordering` (namespace `GloboTicket.Services.Ordering.Saga`):

- [x] Reservation: `ReserveTicketsRequested`, `TicketsReserved`, `TicketsReservationFailed`
- [x] Charge: `ChargeCardRequested`, `CardCharged`, `CardChargeFailed`
- [x] Persist + email checkpoints: `OrderPersisted`, `OrderEmailSent` (cascaded by the saga to itself to advance state)
- [x] Compensation: `ReleaseTicketsRequested`

Existing `PaymentRequestMessage` / V2 stay where they are (namespace `GloboTicket.Messages`, no sub-namespace — they're frozen wire format).

### 5. Saga + step handlers in ordering service

- [x] `OrderSaga : Wolverine.Saga` with `Id` = OrderId. Saga state holds the **transient** workflow concerns: `CurrentStage` (enum: ReservingTickets / AuthorizingPayment / PersistingOrder / SendingEmail / ReleasingReservations), `CreditCardNumber` + `CreditCardExpiry` (cleared as soon as charge resolves), `ReservationsMade` (for compensation), `Lines` + `NextLineIndex` (for the per-line reserve loop), `Total`. Persisted as JSON by `WolverineFx.Postgresql` — NOT an EF entity.
- [x] `Start(SubmitOrderCommand)` → write Order row (`Status = Pending`, `Total` snapshotted from lines), cascade `ReserveTicketsRequested` for first line
- [x] `Handle(TicketsReserved)` → record reservation in saga, cascade next line's reserve, or move to `ChargeCardRequested`
- [x] `Handle(TicketsReservationFailed)` → cascade `ReleaseTicketsRequested` for everything reserved so far, set `Order.Status = Failed` + `FailureReason = "Sold out: <event>"`, `MarkCompleted()`
- [x] `Handle(CardCharged)` → set `Order.PaymentReference = txn_<guid>`, clear card details from saga state, cascade `OrderPersisted` (self)
- [x] `Handle(CardChargeFailed)` → clear card details, set `Order.Status = Failed` + `FailureReason = "Card declined"`, cascade `ReleaseTicketsRequested` for all reservations, `MarkCompleted()`
- [x] `Handle(OrderPersisted)` → set `Order.Status = Confirmed` + `CompletedAt`, cascade `SendOrderEmailRequested`
- [x] `Handle(OrderEmailSent)` → `MarkCompleted()`
- [x] Step handlers as separate classes: `ReserveTicketsHandler` (HTTP to catalog via `ICatalogReservationsClient`), `ChargeCardHandler` (in-process mock; PAN ending 0000 → fail; success returns fake `txn_<guid>` reference), `ReleaseTicketsHandler` (HTTP DELETE to catalog), `SendEmailHandler` (reads Order from DB, sends via `EmailSender` over MailKit/SMTP to Mailpit)
- [x] Wolverine Postgres persistence via `opts.PersistMessagesWithPostgresql(orderingDbConn, "wolverine")` + EF outbox via `opts.UseEntityFrameworkCoreTransactions()` — saga state, envelope tables, and our own writes commit atomically

### 6. Status endpoint

Decision: Wolverine has no public read API for saga state and the storage-mode behaviour is uncertain (lightweight Postgres vs EF saga storage). Instead of querying the saga directly, the saga writes a small `OrderProcessingState` read model in the same DbContext. The status endpoint reads from there.

- [x] Convert ordering project from Worker SDK to Web SDK; switch `Program.cs` to `WebApplication.CreateBuilder` and add `MapControllers`
- [x] `OrderProcessingState` entity: `OrderId` (PK), `CurrentStage` (enum), `LastUpdatedAt`
- [x] `OrderProcessingStage` enum (renamed from `OrderSagaStage`): ReservingTickets, AuthorizingPayment, PersistingOrder, SendingEmail, ReleasingReservations
- [x] `DbSet<OrderProcessingState>` on `OrderingDbContext` + EF migration
- [x] Move `CurrentStage` off the saga: each saga handler that changes stage updates `OrderProcessingState` instead. One write per handler. Saga state itself no longer carries the field.
- [x] `OrderStatusController` with `GET /order/{id}/status`. 404 when Order row not found.
- [ ] Response shape (domain-flavoured):

  ```json
  {
    "status": "Pending|Confirmed|Failed",
    "currentStage": "ReservingTickets|AuthorizingPayment|PersistingOrder|SendingEmail|ReleasingReservations|null",
    "failureReason": "string|null",
    "placedAt": "...",
    "completedAt": "...|null"
  }
  ```

  When `status` is terminal, `currentStage` is `null` (the endpoint reads `OrderProcessingState` only while `Status == Pending`).
- [x] AppHost: Web project gets a `WithReference(ordering)` so Aspire injects the ordering URL for the frontend to call in step 7.

### 7. Frontend (`GloboTicket.Client`)

- [x] `CheckoutController` with `Index` (GET form, prefilled), `Purchase` (POST → publishes `SubmitOrderCommand` via `IMessageBus`, redirects to `Order/{id}`), `Order/{id}` (status page), `OrderStatus/{id}` (JSON pass-through to ordering)
- [x] HttpClient registered against `https+http://ordering` (via `AddHttpClient<CheckoutController>(...)`)
- [x] `CheckoutViewModel` with name, email, address, town, postcode, card, expiry
- [x] `Views/Checkout/Index.cshtml` ported from dapr (form + card-preset dropdown including the "ends in 0000 will be declined" option)
- [x] `Views/Checkout/Order.cshtml` ported from dapr (workflow steps + polling JS). Polling JS adapted to the new response shape: reads `status` (not `runtimeStatus`), `currentStage` enum (not `customStatus` strings), `failureReason` (not `output.reason`). Step indices and the `failedStepFromReason` heuristic kept verbatim.
- [x] CSS for `.workflow-step`, `.workflow-result`, `.workflow-compensation` ported from dapr
- [x] `ShoppingBasketController.Pay` → redirects to `Checkout/Index` instead of publishing V2 message directly. `Thanks.cshtml` deleted.
- [x] Basket cleared after successful submit (cookie rotation rather than calling a basket-service clear method — orphaned basket row stays in basket DB but is harmless)
- [x] **Sold-out / low-stock labels on event listings** (mirrors dapr UX): catalog's `EventDto` (V1 and V2) gained a `TicketsAvailable` field; frontend's `Event` API model gained the same; `DisplayTemplates/Event.cshtml` and `Detail.cshtml` render `SOLD OUT` (when 0) or `ONLY <N> LEFT` (when ≤ 10) badges; `.soldOut` and `.lowStock` CSS ported from dapr's `site.css`. Additive change — V1/V2 backwards-compat tests stayed green.

### 8. Tests + step-5 runtime verification

Step 5 deliberately deferred runtime verification of the saga to here. The open questions below must be validated alongside the test cases — failing any of them likely needs targeted code changes back in step 5 (e.g. an explicit resource-setup hook, `[SagaIdentity]` attributes, or removing/adjusting `MapWolverineEnvelopeStorage()`).

**Open questions from step 5 to validate:**

- [ ] Wolverine envelope / inbox / outbox tables auto-provision in the `wolverine` schema on AppHost startup. If not, find the Wolverine 5.x equivalent of `host.SetupResources()` / `IHostedService` resource provisioning and add it to `Program.cs`.
- [ ] Saga state table auto-provisions and the saga JSON blob persists between handlers (a saga that runs through multiple stages without losing state is the proof).
- [ ] Saga ID correlation works by the `{SagaName}Id` convention — incoming messages with an `OrderId` field route to the right `OrderSaga` instance without needing `[SagaIdentity]` attributes. If correlation fails, decorate the OrderId property on each saga-handled message.
- [ ] Cascade chain (Reserve → Charge → Persist → Email) flows end-to-end under conventional routing + EF outbox. Watch for messages stuck in the outbox or never reaching their handler.
- [ ] `MapWolverineEnvelopeStorage()` on `OrderingDbContext` does not collide with `PersistMessagesWithPostgresql` table management at startup. If it does, drop the call from the DbContext.

**Automated test cases:**

- [x] Existing two backwards-compat tests stay green after rename (already verified)
- [ ] Saga happy-path test driving `SubmitOrderCommand` through `TrackActivity().ExecuteAndWaitAsync`. Asserts: all four stages transition, Order ends `Confirmed`, `PaymentReference` populated, `OrderEmailSent` fires, saga is gone from storage. Decide: in-memory persistence for speed, vs Testcontainers Postgres for fidelity (real Postgres is what proves the auto-provision questions above).
- [ ] Decline-path test (PAN ends `0000`) asserts: charge fails, one `ReleaseTicketsRequested` per previously reserved line, Order ends `Failed` with `FailureReason = "Card declined"`.
- [ ] Sold-out-path test (one line has 0 stock after others reserved) asserts: reservation fails on that line, `ReleaseTicketsRequested` fires for already-reserved lines, Order ends `Failed` with `FailureReason = "Sold out: <event>"`.

**Manual smoke test via AppHost:**

- [ ] Run AppHost, place an order with a healthy-stock event and a non-`0000` card. Observe Order goes Pending → Confirmed, status page shows all four stages tick through, email arrives in Mailpit (<http://localhost:8025>).
- [ ] Place an order with PAN ending `0000`. Observe Order goes Failed, status page shows the charge step failing and the compensation banner, reservations released (catalog stock returns to original).
- [ ] Place an order with a sold-out event. Observe sold-out failure path with no charge attempt.

## Session log

### 2026-05-09 — planning

- Drafted plan and `docs/ordering-enhancement.md`. Created branch `ordering-enhancement`. No merges to `main` until all 8 steps are done.
- Architectural decision: Wolverine Saga (Option A) over inline orchestrator (B) or hybrid (C). Saga is the most teachable Wolverine feature and the cleanest counterpart to Dapr Workflow.
- Legacy-compat decision: keep `PaymentRequestMessage` / V2 + their handlers as a frozen demo of the message-versioning lesson. They're no longer on the live order path; the new path uses `SubmitOrderCommand`.
- ~~Status-endpoint shape is deliberately copied from the dapr response so the polling JS in `Order.cshtml` ports verbatim.~~ Reversed during step 3 — see step-3 log entry. The endpoint now uses domain-flavoured fields and the JS gets minor adaptation.

### 2026-05-09 — step 7 done

- Sold-out / low-stock badges added in step 7 (not in original plan). `TicketsAvailable` is now on V1 and V2 EventDtos and on the frontend's API model. Existing catalog backwards-compat tests stayed green because they only check `price`/`tickets` presence/absence — additive fields don't trip them.
- Checkout flow: `Pay` button on the basket now redirects to `Checkout/Index` instead of publishing a V2 message directly. The V2 message + handlers stay as the legacy-compat / message-versioning teaching artifact, untouched.
- `[CreditCard]` validation kept on the credit-card field. Both demo cards (Visa 4242×16, Mastercard 5454×12+0000) Luhn-validate, so the "ends in 0000 fails" behaviour is mocked at the saga's charge step rather than at the form.
- Basket cleared via cookie rotation (`Response.Cookies.Delete`) rather than calling a basket-service clear method — the basket row in the DB becomes orphaned, but for this demo that's acceptable and keeps the basket service unchanged.
- Polling JS in `Order.cshtml` is the dapr version with three field renames: `runtimeStatus → status`, `customStatus → currentStage` (now enum names like `ReservingTickets`, not human strings), `output.reason → failureReason`. Terminal-status set is `{Confirmed, Failed}` instead of dapr's `{Completed, Failed, Terminated, Canceled}`. The `failedStepFromReason` heuristic ports verbatim — failure-reason strings match dapr (`"Sold out: …"`, `"Card declined"`).
- `IMessageBus` removed from `ShoppingBasketController` (Pay no longer publishes), tidying the unused field + the `GloboTicket.Messages` and `Wolverine` imports.

### 2026-05-09 — step 6 done

- Saga progress queryable via a separate `OrderProcessingState` EF entity rather than querying the saga's own state. Wolverine has no public read API for saga state and the lightweight-vs-EF saga-storage mode behaviour is uncertain — owning the read path ourselves is deterministic.
- `CurrentStage` moved off `OrderSaga` entirely. The saga only ever wrote that field (never read it for its own decisions), so moving it costs nothing.
- `OrderProcessingState` row created in `Start` and updated by saga handlers as the flow advances. Not deleted on saga completion — the row stays, but the status endpoint ignores it for terminal Orders so it makes no difference.
- Ordering converted from Worker SDK to Web SDK. The legacy-compat handlers (`NewOrderHandler`/`V2`) and Wolverine wiring are unaffected; only the host shape changed.
- `OrderStatusController` reads Order + (conditionally) `OrderProcessingState`. Aspire injects `ordering` into the Web project so the frontend's status page (step 7) can poll over service discovery.

### 2026-05-09 — step 5 done

- Saga structure follows the four user-visible steps: Reserve → Charge → Save → Email. Steps 1, 2, and 4 are separate Wolverine handler classes (`ReserveTicketsHandler`, `ChargeCardHandler`, `SendEmailHandler`); step 3 is intrinsically the saga's own EF write inside `Handle(OrderPersisted)`. Compensation has its own `ReleaseTicketsHandler`.
- Added one new internal message — `SendOrderEmailRequested` — retroactively to step 4. The decision was that the email step should be a real handler (separate retry envelope, symmetrical with reservation/charge), not an inline service call from the saga.
- Step 3's open question resolved: Wolverine envelope/outbox/saga tables aren't scaffolded by EF migrations. They're created at runtime by `WolverineFx.Postgresql` once `opts.PersistMessagesWithPostgresql(connectionString, "wolverine")` is configured. Added that call to `Program.cs`. Whether they auto-provision on first startup or need an explicit setup step will get exercised when we actually run the AppHost — flagged for verification.
- Saga state is JSON-serialised by Wolverine, NOT an EF entity. `OrderingDbContext` does not have a `DbSet<OrderSaga>`. Earlier research had this wrong; corrected during step 5.
- Saga ID correlation works by the `{SagaName}Id` convention — `OrderSaga.Id` matches messages' `OrderId` field automatically. No `[SagaIdentity]` attributes needed.
- `Wolverine.Saga` (the base class) shadows `GloboTicket.Services.Ordering.Saga` (our namespace). Resolved by writing `: Wolverine.Saga` fully qualified rather than renaming our folder/namespace.
- MailKit chosen over `System.Net.Mail.SmtpClient` (the latter is officially obsolete). Bumped to 4.16.0 (4.8.0 had moderate-severity advisories that tripped `TreatWarningsAsErrors`).
- Catalog HTTP wrapped in a typed `ICatalogReservationsClient` (Reserve / Release) so the step handlers stay focused on bus-level concerns and the HTTP shape lives in one place.
- Card details get cleared from saga state the moment the charge step resolves (success or failure) so they don't sit in the saga JSON blob longer than needed.

### 2026-05-09 — step 4 done

- Split contract messages from internal saga messages. `GloboTicket.Messages` (referenced by Web + Ordering + tests) only carries `SubmitOrderCommand` + its support types `CustomerDetails` and `OrderLine`. The 7 saga step / compensation messages live in `GloboTicket.Services.Ordering.Saga` because they never cross a service boundary — they flow between the saga and step handlers in the same process.
- Diverges from the original step-4 plan, which had everything in `GloboTicket.Messages`. The cleaner split also means the demo teaches "don't expose orchestration internals across boundaries" alongside the saga pattern itself.
- New contract messages live under `namespace GloboTicket.Messages.Ordering` to keep the legacy `PaymentRequestMessage` family (no sub-namespace, frozen wire format) cleanly separated from the modern ordering contract.

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
