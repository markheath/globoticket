# GloboTicket

A sample ASP.NET Core microservices application used in the Pluralsight .NET Microservices learning path.

## Architecture

Four services orchestrated by .NET Aspire:

- **GloboTicket.Web** — MVC front-end. Calls the EventCatalog and ShoppingBasket APIs and publishes payment messages.
- **GloboTicket.Services.EventCatalog** — events/categories/tickets API, backed by SQL Server. Demonstrates URL-versioned APIs (`v1` returns a flat `price`, `v2` returns a `tickets` collection).
- **GloboTicket.Services.ShoppingBasket** — basket API, backed by SQL Server.
- **GloboTicket.Services.Payment** — Worker that consumes payment messages from RabbitMQ. Demonstrates message contract versioning (`PaymentRequestMessage` and `PaymentRequestMessageV2`).

The **GloboTicket.AppHost** project orchestrates the whole graph — SQL Server, RabbitMQ, and the four services — through .NET Aspire.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A container runtime (Docker Desktop, Podman Desktop, or Rancher Desktop) — Aspire runs SQL Server and RabbitMQ in containers.

## Build, test, run

```bash
cd GloboTicket
dotnet build
dotnet test
dotnet run --project GloboTicket.AppHost
```

`dotnet run` opens the Aspire dashboard. Once SQL Server and RabbitMQ are healthy, click the `web` resource to open the site. The first run pulls container images and may take a minute.

## What's where

| Concern | Where |
|---|---|
| Service orchestration, container resources, service discovery | `GloboTicket.AppHost/AppHost.cs` |
| Shared OpenTelemetry, health checks, resilience | `GloboTicket.ServiceDefaults/Extensions.cs` |
| API versioning | `Asp.Versioning.Mvc` (query string `?api-version=2.0`) |
| OpenAPI docs | `Microsoft.AspNetCore.OpenApi` + Scalar UI at `/scalar/v1` |
| Messaging | Wolverine on RabbitMQ |
| Tests | `*.IntegrationTests` (WebApplicationFactory) and `*.Tests` (Wolverine tracked-session harness) |

## Backwards-compatibility lessons

- **API versioning** — see `EventController` (v1) and `Controllers/V2/EventController.cs` (v2). Backing tests live in `GloboTicket.Services.EventCatalog.IntegrationTests`.
- **Message versioning** — see `PaymentRequestMessage` / `PaymentRequestMessageV2` in `GloboTicket.Messages` and the two consumers in `GloboTicket.Services.Payment`. Backing tests live in `GloboTicket.Services.Payment.Tests`.
