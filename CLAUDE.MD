# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 9 · Aspire 9.5 · PostgreSQL · RabbitMQ · Redis · YARP · Hangfire

## Services

| Service      | Purpose                                   |
| ------------ | ----------------------------------------- |
| Identity     | Auth, JWT, OTP, user management           |
| Vehicle      | Vehicle tracking, maintenance, scheduling |
| Media        | File upload, storage, asset serving       |
| Notification | Email/SMS, SignalR hubs, event-driven     |
| Ai           | Gemini analysis, onboarding questionnaire |

Each service: 4 projects — `Verendar.{Service}` (Host), `.Application`, `.Domain`, `.Infrastructure`.
Special: `Verender.Identity.Contracts` and `Verendar.Vehicle.Contracts` hold shared MassTransit event contracts.

## Commands

```bash
dotnet run --project App/Verendar.AppHost        # run all services + infra via Aspire
dotnet build Verendar.sln                        # build check

# EF Core migration (from repo root)
dotnet ef migrations add <Name> \
  --project {Service}/Verendar.{Service}.Infrastructure \
  --startup-project {Service}/Verendar.{Service}
```

Prerequisites: .NET 9 SDK, Docker.

## Gateway Routing (port 8080)

| URL prefix                                                                                                                                                                            | Service      |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
| `/api/v1/auth/**`, `/api/v1/users/**`, `/api/internal/**`                                                                                                                             | Identity     |
| `/api/v1/ai/**`                                                                                                                                                                       | Ai           |
| `/api/v1/user-vehicles/**`, `/api/v1/maintenance-records/**`, `/api/v1/vehicle-models/**`, `/api/v1/brands/**`, `/api/v1/parts/**`, `/api/v1/types/**`, `/api/v1/odometer-history/**` | Vehicle      |
| `/api/v1/media-files/**`                                                                                                                                                              | Media        |
| `/api/v1/notifications/**`, `/hubs/**`                                                                                                                                                | Notification |

## Non-Negotiable Patterns

| Rule           | Detail                                                                 |
| -------------- | ---------------------------------------------------------------------- |
| No Controllers | Minimal API: `MapGroup` → `MapXxxRoutes()` → private handlers          |
| No AutoMapper  | Static extensions: `ToEntity()`, `ToResponse()`, `UpdateFromRequest()` |
| No MediatR     | Services call `IUnitOfWork` repositories directly                      |
| Response       | `ApiResponse<T>` on all endpoints; FluentValidation/500s → RFC 7807    |
| Validation     | Structural rules in `AbstractValidator<T>`, business rules in service  |
| Soft delete    | `DeletedAt = DateTime.UtcNow` — never `DbContext.Remove()`             |
| Lists          | `PaginationRequest` + `GetPagedAsync` — never unbounded                |
| Secrets        | User Secrets only — never `appsettings.json`                           |
| Constructor    | Primary constructor + private readonly fields (project convention)     |
| Async          | `async/await` throughout — never `.Result` or `.Wait()`                |

## Key Exceptions

- **Identity service implementations** live in `Infrastructure/Services/` (not Application) — they depend on `PasswordHasher<User>` and MassTransit `IPublishEndpoint`.
- **Hangfire dashboard** and **test user seeding** are guarded by `IsDevelopment()`.
- **AI health check** (`GET /api/v1/ai/health`) returns raw `HealthCheckResponse`, not `ApiResponse<T>`.

## AI-Assisted Onboarding Flow

1. `POST /api/v1/user-vehicles` — create vehicle (`NeedsOnboarding = true`)
2. `GET /api/v1/vehicle-models/{id}/part-categories/{code}/default-schedule` — manufacturer schedule
3. `POST /api/v1/ai/vehicle-questionnaire/analyze` — AI analyzes one part → estimates + confidence
4. `POST /api/v1/user-vehicles/{id}/apply-tracking` — write `VehiclePartTracking` with AI result
5. Repeat 2–4 per part
6. `PATCH /api/v1/user-vehicles/{id}/complete-onboarding` — `NeedsOnboarding = false`

AI reasoning stored in `VehiclePartTracking.AiAnalysisResult` (JSON). Vietnamese prompts for Gemini (token cost).

## Skills

Load `.claude/skills/backend-development/` for detailed patterns and code examples:

| Need                                               | File                         |
| -------------------------------------------------- | ---------------------------- |
| API structure, routes, handlers, pagination        | `references/api-design.md`   |
| Layer responsibilities, what goes where, checklist | `references/architecture.md` |
| SOLID, clean code, refactoring                     | `references/code-quality.md` |
| N+1, caching, query hygiene                        | `references/performance.md`  |
| Tests                                              | `references/testing.md`      |
