# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 9 · Aspire 9.5 · PostgreSQL · RabbitMQ · Redis · YARP · Hangfire

## Services

| Service      | Purpose                                                    |
| ------------ | ---------------------------------------------------------- |
| Identity     | Auth, JWT, OTP, user management                            |
| Vehicle      | Vehicle tracking, maintenance, scheduling                  |
| Media        | File upload, storage, asset serving                        |
| Notification | Email/SMS, SignalR hubs, event-driven                      |
| Ai           | Gemini analysis, onboarding questionnaire                  |
| Garage       | Garage accounts, branches, products, bookings, mechanics   |
| Payment      | VNPay integration, payment lifecycle, webhook handling     |
| Location     | Vietnamese administrative regions, provinces, wards lookup |

Each service: 4 projects — `Verendar.{Service}` (Host), `.Application`, `.Domain`, `.Infrastructure`.
Location additionally has `Verendar.Location.Tests` (unit tests — see testing patterns below).
Special: `Verendar.Identity.Contracts` and `Verendar.Vehicle.Contracts` hold shared MassTransit event contracts.

## Commands

```bash
task app:run   # run all services + infra via Aspire
task app:build # build all services and infrastructure
task app:clean # remove Aspire containers and volumes

# EF Core migration
task migrate:add NAME={MIGRATION_NAME} PROJECT={PROJECT} STARTUP={STARTUP}
task migrate:remove PROJECT={PROJECT} STARTUP={STARTUP}

# Docker Compose (alternative to Aspire)
task docker:dev:up      # start dev stack
task docker:dev:down    # stop dev stack (keeps volumes)
task docker:dev:down:v  # stop dev stack and delete volumes

# Tests
dotnet test Location/Verendar.Location.Tests
dotnet test Location/Verendar.Location.Tests --filter "FullyQualifiedName~ProvinceService"

# Other
task pull br={branch}  # pull from remote
```

Prerequisites: .NET 9 SDK, Docker.

## Gateway Routing (port 8080)

| Service      | Routes                                                                                                                                                                   |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Identity     | `/api/v1/auth/**` · `/api/v1/users/**` · `/api/v1/subscriptions/**` · `/api/internal/**`                                                                               |
| Ai           | `/api/v1/ai/**`                                                                                                                                                          |
| Vehicle      | `/api/v1/brands/**` · `/api/v1/types/**` · `/api/v1/models/**` · `/api/v1/variants/**` · `/api/v1/vehicle-models/**` · `/api/v1/user-vehicles/**` · `/api/v1/maintenance-records/**` · `/api/v1/odometer-history/**` · `/api/v1/part-categories/**` · `/api/v1/part-products/**` |
| Media        | `/api/v1/media-files/**`                                                                                                                                                 |
| Notification | `/api/v1/notifications/**` · `/hubs/**`                                                                                                                                 |
| Garage       | `/api/v1/garages/**` · `/api/v1/bookings/**` · `/api/v1/mechanics/**`                                                                                                   |
| Payment      | `/api/v1/payments/**`                                                                                                                                                    |
| Location     | `/api/v1/locations/**`                                                                                                                                                   |

## Non-Negotiable Patterns

| Rule           | Detail                                                                 |
| -------------- | ---------------------------------------------------------------------- |
| No Controllers | Minimal API: `MapGroup` → `MapXxxRoutes()` → private handlers          |
| No AutoMapper  | Static extensions: `ToEntity()`, `ToResponse()`, `UpdateFromRequest()` |
| No MediatR     | Services call `IUnitOfWork` repositories directly                      |
| Response       | `ApiResponse<T>` on all endpoints; FluentValidation/500s → RFC 7807    |
| Validation     | Structural rules in `AbstractValidator<T>`, business rules in service  |
| Soft delete    | `DeletedAt = DateTime.UtcNow` — never `DbContext.Remove()`             |
| Lists          | `PaginationRequest` + `GetPagedAsync` — never unbounded (read-only reference data may return full lists) |
| Secrets        | User Secrets only — never `appsettings.json`                           |
| Constructor    | Primary constructor + private readonly fields (project convention)     |
| Async          | `async/await` throughout — never `.Result` or `.Wait()`                |

## Key Exceptions

- **Identity service implementations** live in `Infrastructure/Services/` (not Application) — they depend on `PasswordHasher<User>` and MassTransit `IPublishEndpoint`.
- **Hangfire dashboard** and **test user seeding** are guarded by `IsDevelopment()`.
- **AI health check** (`GET /api/v1/ai/health`) returns raw `HealthCheckResponse`, not `ApiResponse<T>`.
- **Location service** does not use `PaginationRequest` for its list endpoints — data is static reference data with Redis caching (24h TTL). Unbounded lists are acceptable here.
- **Internal endpoints** (`/api/internal/...`) skip `ApiResponse<T>` and return domain-specific shapes for service-to-service calls (e.g., `LocationValidationResult`).
- **Payment service** owns `IPaymentGateway` / `VNPayGateway` — Garage and Identity call Payment via `IPaymentClient` (HTTP) and consume `PaymentSucceededEvent` / `PaymentRefundedEvent` via MassTransit. Never implement VNPay directly in Garage or Identity.

## AI-Assisted Onboarding Flow

1. `POST /api/v1/user-vehicles` — create vehicle (`NeedsOnboarding = true`)
2. `GET /api/v1/vehicle-models/{id}/part-categories/{code}/default-schedule` — manufacturer schedule
3. `POST /api/v1/ai/vehicle-questionnaire/analyze` — AI analyzes one part → estimates + confidence
4. `POST /api/v1/user-vehicles/{id}/apply-tracking` — write `VehiclePartTracking` with AI result
5. Repeat 2–4 per part
6. `PATCH /api/v1/user-vehicles/{id}/complete-onboarding` — `NeedsOnboarding = false`

AI reasoning stored in `VehiclePartTracking.AiAnalysisResult` (JSON). Vietnamese prompts for Gemini (token cost).

## Testing

The **Location service** is the reference implementation for unit tests (`Location/Verendar.Location.Tests`).
Use xUnit + Moq + FluentAssertions. See `.claude/skills/backend-development/references/testing.md` for full patterns.

Key conventions:
- Test naming: `MethodName_WhenCondition_ExpectedBehavior`
- `XxxUnitOfWorkMock` helper wires up `Mock<IUnitOfWork>` + individual `Mock<IXxxRepository>(MockBehavior.Strict)`
- `Mock<ICacheService>(MockBehavior.Strict)` — must declare all cache interactions explicitly
- `XxxServiceResponseAssert` static helper validates `ApiResponse<T>` envelope fields
- Cover: cache hit (skip repo), cache miss (load + set cache), not found (404), exception → failure envelope

## Skills

Load `.claude/skills/backend-development/` for detailed patterns and code examples:

| Need                                               | File                         |
| -------------------------------------------------- | ---------------------------- |
| API structure, routes, handlers, pagination        | `references/api-design.md`   |
| Layer responsibilities, what goes where, checklist | `references/architecture.md` |
| SOLID, clean code, refactoring                     | `references/code-quality.md` |
| N+1, caching, query hygiene                        | `references/performance.md`  |
| Tests (xUnit + Moq + FluentAssertions)             | `references/testing.md`      |
