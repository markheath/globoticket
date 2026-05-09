# GloboTicket

A sample ASP.NET Core microservices application used in the Pluralsight .NET Microservices learning path.

## Architecture

Four services orchestrated by .NET Aspire:

- **GloboTicket.Web** — MVC front-end. Calls the EventCatalog and ShoppingBasket APIs, drives the checkout form, and submits orders to the Ordering service via `IMessageBus.InvokeAsync<OrderResult>` (request/response over RabbitMQ).
- **GloboTicket.Services.EventCatalog** — events/categories/tickets API, backed by Postgres. Demonstrates URL-versioned APIs (`v1` returns a flat `price`, `v2` returns a `tickets` collection). Also exposes ticket reservation endpoints (`POST` / `DELETE /api/events/{id}/reserve`) that decrement stock atomically.
- **GloboTicket.Services.ShoppingBasket** — basket API, backed by Postgres.
- **GloboTicket.Services.Ordering** — runs the order flow inline: reserve stock per line, mock-charge the card (PAN ending `0000` declines), persist the Order row, send a confirmation email through MailPit. Stock is released as compensation on any failure. Also hosts `GET /order/{id}` for the frontend's result page and the legacy `PaymentRequestMessage` / `V2` consumers retained as a message-versioning teaching artifact.

The **GloboTicket.AppHost** project orchestrates the whole graph — Postgres, RabbitMQ, MailPit, and the four services — through .NET Aspire.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A container runtime (Docker Desktop, Podman Desktop, or Rancher Desktop) — Aspire runs Postgres, RabbitMQ, and MailPit in containers.

## Build, test, run

```bash
cd GloboTicket
dotnet build
dotnet test
dotnet run --project GloboTicket.AppHost
```

`dotnet run` opens the Aspire dashboard. Once Postgres, RabbitMQ, and MailPit are healthy, click the `web` resource to open the site. The first run pulls container images and may take a minute. Sent emails land in MailPit's web UI on <http://localhost:8025>.

## What's where

| Concern | Where |
|---|---|
| Service orchestration, container resources, service discovery | `GloboTicket.AppHost/AppHost.cs` |
| Shared OpenTelemetry, health checks, resilience | `GloboTicket.ServiceDefaults/Extensions.cs` |
| API versioning | `Asp.Versioning.Mvc` (query string `?api-version=2.0`) |
| OpenAPI docs | `Microsoft.AspNetCore.OpenApi` + Scalar UI at `/scalar/v1` |
| Messaging | Wolverine on RabbitMQ (publish/subscribe + request/response) |
| Order flow | `GloboTicket.Services.Ordering/SubmitOrderHandler.cs` |
| Tests | `*.IntegrationTests` (WebApplicationFactory) and `*.Tests` (Wolverine tracked-session harness) |

## Backwards-compatibility lessons

- **API versioning** — see `EventController` (v1) and `Controllers/V2/EventController.cs` (v2). `TicketsAvailable` was added to both DTOs without breaking either contract. Backing tests live in `GloboTicket.Services.EventCatalog.IntegrationTests`.
- **Message versioning** — see `PaymentRequestMessage` / `PaymentRequestMessageV2` in `GloboTicket.Messages` and the two consumers in `GloboTicket.Services.Ordering` (`NewOrderHandler`). They're frozen in place — no longer on the live order path, but kept as the teaching artifact. Backing tests live in `GloboTicket.Services.Ordering.Tests`.
