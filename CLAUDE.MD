# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Verendar — vehicle maintenance platform built as .NET 9 microservices with Aspire 9.5.
Stack: **PostgreSQL · RabbitMQ · Redis · YARP · Hangfire**
Services: **Identity · Vehicle · Media · Notification · Ai · Garage · Payment · Location**

## Critical Rules

### 1. API Layer

- Minimal API only — route groups in `{Module}Apis.cs`, handlers as private static methods
- `ApiResponse<T>` on every public endpoint; errors map to RFC 7807
- Structural validation via `ValidationEndpointFilter.Validate<T>()` as endpoint filter; business rules stay in service layer
- Internal endpoints (`/api/internal/...`) skip `ApiResponse<T>` and authenticate via `IServiceTokenProvider`
- All list endpoints use `PaginationRequest` with `[AsParameters]`; exception: Location service (unbounded, 24 h Redis cache)

### 2. Domain & Data

- All entities inherit `BaseEntity` (UUID v7 `Id`, `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By`)
- **Soft delete only** — `entity.DeletedAt = DateTime.UtcNow`; never `DbContext.Remove()`
- `BaseDbContext` auto-applies global query filter (`DeletedAt == null`) and populates audit fields on `SaveChangesAsync`
- DTO mapping via static extensions only: `ToEntity()`, `ToResponse()`, `UpdateFromRequest()`
- No AutoMapper, no MediatR — call `IUnitOfWork` repositories directly from services

### 3. Security

- Secrets in User Secrets only — never `appsettings.json`
- Every public endpoint must have `.RequireAuthorization()` or explicit public annotation
- Payment (VNPay) logic lives exclusively in the Payment service; other services use `IPaymentClient` + consume `PaymentSucceededEvent` / `PaymentRefundedEvent`

### 4. Testing

- TDD: write failing tests before implementation
- 80% minimum coverage overall; 100% for Payment and Identity auth logic
- Unit tests: xUnit + FluentAssertions + NSubstitute
- Integration tests: Testcontainers (real PostgreSQL) — never in-memory DB
- Consumer tests: MassTransit TestHarness

## Service Structure

Each service is a 4-project group:

```
{Service}/
├── Verendar.{Service}                # Host — Program.cs, Apis/, Bootstrapping/
├── Verendar.{Service}.Application    # Services, DTOs, Validators
├── Verendar.{Service}.Domain         # Entities, IRepositories, IUnitOfWork
└── Verendar.{Service}.Infrastructure # DbContext, Repositories, Migrations
```

Shared code: `App/Verendar.Common` · `App/Verendar.ServiceDefaults`
Event contracts: `Verendar.{Service}.Contracts` — all events extend `BaseEvent`; consumers are auto-discovered

## Key Patterns

### Service Bootstrap

```csharp
// Every Host Program.cs
builder.AddServiceDefaults();
builder.AddCommonService();
builder.AddPostgresDatabase<TDbContext>();
builder.AddServiceRedis(); // optional

// Then:
app.UseApplicationServices();
// map route groups
```

### Inter-Service Communication

```csharp
// Async — MassTransit/RabbitMQ (prefer for fire-and-forget, notifications)
await _publishEndpoint.Publish(new BookingCreatedEvent { ... }, ct);

// Sync — typed HTTP client (prefer for request/response, real-time data)
// Aspire service discovery: https+http://service-name
var result = await _garageClient.GetGarageAsync(garageId, ct);
```

### Endpoint Structure

```csharp
public static class BookingApis
{
    public static RouteGroupBuilder MapBookingApis(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateHandler)
             .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateBookingRequest>())
             .RequireAuthorization();
        return group;
    }

    private static async Task<IResult> CreateHandler(
        CreateBookingRequest req, IBookingService svc, CancellationToken ct)
    {
        var result = await svc.CreateAsync(req, ct);
        return result.IsSuccess
            ? Results.Created($"/bookings/{result.Data!.Id}", result)
            : Results.Problem(result.ToProblemDetails());
    }
}
```

## Available Commands

- `/plan` — Create step-by-step implementation plan before writing code
- `/tdd` (alias: `/tdd-workflow`) — TDD workflow with RED→GREEN→REFACTOR
- `/code-review` — Review code quality, Verendar constraints, and async patterns
- `/build-fix` — Diagnose and fix build errors
- `/feature-dev` — Guided feature development with codebase exploration

## Task Commands

```bash
task run              # start all services + infra via Aspire
task build            # build solution
task test:all         # run all tests
task test PROJECT=Location/Verendar.Location.Tests
task clear            # remove Aspire containers and volumes

task migrate:add NAME=<MigrationName> PROJECT=<Service>/Verendar.<Service>.Infrastructure STARTUP=<Service>/Verendar.<Service>
task migrate:remove PROJECT=<Service>/Verendar.<Service>.Infrastructure STARTUP=<Service>/Verendar.<Service>
```

## Git Workflow

- Conventional commits: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`
- Never commit directly to `main` — always work on `dev` or feature branches
- All tests must pass before merge
