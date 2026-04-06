# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 9 · Aspire 9.5 · PostgreSQL · RabbitMQ · Redis · YARP · Hangfire

## Commands

```bash
task run              # start all services + infra via Aspire
task build            # build solution
task test:all         # run all tests
task test PROJECT=Location/Verendar.Location.Tests
task clear            # remove Aspire containers and volumes

task migrate:add NAME=<MigrationName> PROJECT=<Service>/Verendar.<Service>.Infrastructure STARTUP=<Service>/Verendar.<Service>
task migrate:remove PROJECT=<Service>/Verendar.<Service>.Infrastructure STARTUP=<Service>/Verendar.<Service>
```

## System Structure

Each service is a 4-project group: `Verendar.{Service}` (Host) · `.Application` · `.Domain` · `.Infrastructure`.
Shared code: `App/Verendar.Common` (ApiResponse, BaseEntity, ICacheService, IUnitOfWork helpers, middleware) and `App/Verendar.ServiceDefaults` (OpenTelemetry, health checks).

Services: **Identity** · **Vehicle** · **Media** · **Notification** · **Ai** · **Garage** · **Payment** · **Location**

MassTransit event contracts live in dedicated `Verendar.{Service}.Contracts` projects. All events extend `BaseEvent`. Consumers are auto-discovered from referenced assemblies — no manual registration needed.

## Implementation Patterns

**API layer** — Minimal API only. Route groups defined in `{Module}Apis.cs`, handlers are private static methods. Structural validation via `ValidationEndpointFilter.Validate<T>()` as an endpoint filter; business rules stay in the service layer. All responses use `ApiResponse<T>`; errors map to RFC 7807.

**Domain** — All entities inherit `BaseEntity` (provides `Id` as UUID v7, `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By`). Soft delete by setting `DeletedAt = DateTime.UtcNow`. `BaseDbContext` automatically applies a global query filter (`DeletedAt == null`) and populates audit fields on `SaveChangesAsync`. Lists go through `PaginationRequest` + `GetPagedAsync`.

**Application services** — Call `IUnitOfWork` repositories directly, return `ApiResponse<T>`. DTO mapping via static extensions: `ToEntity()`, `ToResponse()`, `UpdateFromRequest()`.

**Inter-service communication** — Two channels:

- **Async (MassTransit/RabbitMQ)**: publish/consume events; exponential retry (5× , 2–30 s)
- **Sync (typed HTTP clients)**: `ILocationClient`, `IPaymentClient`, `IIdentityClient`, etc., registered in each service's `Bootstrapping/`. Aspire service discovery: `https+http://service-name`; override via `Configuration["Services:{Name}:BaseUrl"]`. Internal endpoints (`/api/internal/...`) skip `ApiResponse<T>` and authenticate via `IServiceTokenProvider`.

**Service bootstrap** — every Host follows: `AddServiceDefaults()` → `AddCommonService()` → `AddPostgresDatabase<TDbContext>()` → (optional) `AddServiceRedis()`. Then `Program.cs`: migrate → seed (dev only) → `UseApplicationServices()` → map route groups.

## Constraints

- No controllers, no AutoMapper, no MediatR
- `ApiResponse<T>` on every public endpoint
- Soft delete only — never `DbContext.Remove()`
- Secrets in User Secrets only — never `appsettings.json`
- Paginate all lists (`PaginationRequest`) — exception: Location service uses unbounded lists with 24 h Redis cache
- Payment (VNPay) logic lives exclusively in the Payment service; other services call it via `IPaymentClient` + consume `PaymentSucceededEvent` / `PaymentRefundedEvent`
