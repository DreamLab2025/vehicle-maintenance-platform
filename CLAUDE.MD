# Verendar Project — Claude Context

## Stack

.NET 9 · Aspire 9.5 · PostgreSQL · RabbitMQ · Redis · YARP · Hangfire

## Services

| Service      | Architecture | Purpose                                    |
|--------------|--------------|--------------------------------------------|
| Identity     | Clean Arch   | Auth, JWT, OTP, user management            |
| Vehicle      | Clean Arch   | Vehicle tracking, maintenance, scheduling  |
| Media        | Clean Arch   | File upload, storage, asset serving        |
| Notification | Clean Arch   | Email/SMS, SignalR hubs, event-driven      |
| Ai           | Clean Arch   | Gemini analysis, onboarding questionnaire  |

## Solution Layout

```
Verendar/
├── App/
│   ├── Verendar.AppHost/              # Aspire orchestration entry point
│   │   └── Extensions/               # Per-service AppHost extension methods
│   ├── Verendar.ServiceDefaults/      # Shared service configuration (telemetry, health)
│   ├── Verendar.Common/              # Shared utilities (ApiResponse, Pagination, JWT, etc.)
│   └── Verendar.DatabaseMigrationHelpers/
├── Ai/
│   ├── Verendar.Ai/                  # Host — Minimal API + Bootstrapping
│   ├── Verendar.Ai.Application/      # Use cases, services, DTOs, prompts
│   ├── Verendar.Ai.Domain/           # Entities, enums, repository interfaces
│   └── Verendar.Ai.Infrastructure/   # EF Core, external services (Gemini)
├── Identity/
│   ├── Verendar.Identity/            # Host — Minimal API + Bootstrapping
│   ├── Verendar.Identity.Application/
│   ├── Verendar.Identity.Domain/
│   ├── Verendar.Identity.Infrastructure/
│   └── Verender.Identity.Contracts/  # Shared events (UserRegisteredEvent, etc.)
├── Media/
│   ├── Verendar.Media/               # Host
│   ├── Verendar.Media.Application/
│   ├── Verendar.Media.Domain/
│   └── Verendar.Media.Infrastructure/
├── Notification/
│   ├── Verendar.Notification/        # Host — includes email Templates/
│   ├── Verendar.Notification.Application/ # Consumers, Hubs (SignalR), Services
│   ├── Verendar.Notification.Domain/
│   └── Verendar.Notification.Infrastructure/
├── Vehicle/
│   ├── Verendar.Vehicle/             # Host — includes Jobs/ (Hangfire)
│   ├── Verendar.Vehicle.Application/
│   ├── Verendar.Vehicle.Contracts/   # Shared events
│   ├── Verendar.Vehicle.Domain/
│   └── Verendar.Vehicle.Infrastructure/
├── docker/                           # Docker configs (gateway, postgres)
└── Verendar.sln
```

## Key Patterns

| Pattern | Convention |
|---------|-----------|
| **API** | Minimal API — `MapGroup` + `Map*Endpoints()`. Never Controllers. |
| **DI** | Bootstrapping extension methods in `*/Bootstrapping/`. Not inline in Program.cs. |
| **Mapping** | Static extension methods (`ToResponse()`, `ToEntity()`). No AutoMapper. |
| **Validation** | FluentValidation in `Application/Validators/`. Vietnamese user-facing messages. |
| **Response** | `ApiResponse<T>` from `App/Verendar.Common/Shared/ApiResponse.cs` wraps everything. |
| **Secrets** | User Secrets only. Never in `appsettings.json`. |

## Response Shape

All endpoints return `ApiResponse<T>`:

```json
// Success (single)
{ "isSuccess": true, "message": "Success", "data": { ... }, "metadata": null }

// Success (paginated) — data is List<T>, metadata is PagingMetadata
{ "isSuccess": true, "message": "Success", "data": [ ... ],
  "metadata": { "pageNumber": 1, "pageSize": 10, "totalItems": 3,
                "totalPages": 1, "hasNextPage": false, "hasPreviousPage": false } }

// Business failure
{ "isSuccess": false, "message": "Vehicle not found.", "data": null, "metadata": null }
```

**Exception**: FluentValidation errors and unhandled 500s use RFC 7807 Problem Details (not ApiResponse):
```json
{ "title": "One or more validation errors occurred.", "status": 400,
  "errors": { "fieldName": ["message"] } }
```

## Architecture

**Clean Architecture layers** (all services):
```
Domain → Application → Infrastructure → Host
```
- Domain: entities, value objects, domain events, interfaces
- Application: use cases, validators, DTOs, service interfaces
- Infrastructure: EF Core, repositories, external APIs
- Host: Minimal API endpoints, program entry, bootstrapping

**Communication**:
- Sync: HTTP/REST via YARP reverse proxy
- Async: RabbitMQ domain events (e.g., user registered → notification triggered)

**Database**: One PostgreSQL DB per service. Migrations auto-run on startup via `MigrateDbContextAsync<T>()`.

## AI-Assisted Onboarding Flow

Allows users to initialize vehicle maintenance tracking accurately using AI.

**Steps (all optional after vehicle creation)**:
1. `POST /api/v1/user-vehicles` — creates vehicle with `NeedsOnboarding = true`, NO tracking yet
2. `GET /api/v1/vehicle-models/{vehicleModelId}/part-categories/{code}/default-schedule` — manufacturer schedule
3. Frontend shows questionnaire per part (e.g., "When did you last change oil?")
4. `POST /api/v1/ai/vehicle-questionnaire/analyze` — AI analyzes ONE part, returns estimates + confidence
5. `POST /api/v1/user-vehicles/{id}/apply-tracking` — creates/updates `VehiclePartTracking` with AI result
6. Repeat steps 2–5 for other parts as needed
7. `PATCH /api/v1/user-vehicles/{id}/complete-onboarding` — sets `NeedsOnboarding = false`

**Key decisions**:
- Lazy tracking: records only created when user explicitly analyzes a part
- Single-part per AI request (accuracy > batch)
- AI reasoning stored in `VehiclePartTracking.AiAnalysisResult` (JSON, for audit)
- Vietnamese prompts for Gemini (token cost optimization)

## Running Locally

```bash
dotnet run --project App/Verendar.AppHost   # Starts all services + infrastructure
dotnet build Verendar.sln               # Build check
```

Prerequisites: .NET 9 SDK, Docker (Postgres 17, RabbitMQ 3, Redis Alpine via Aspire).

## Skills Reference

`.claude/skills/backend-development/` — patterns, API design, mindset, performance, devops, code quality.
Load it when making architectural decisions, designing endpoints, or reviewing code.

## Notes

- All services follow Clean Architecture — Identity included
- Never use Controllers — always Minimal API
- No direct DB sharing between services — cross-service data via HTTP or events
- `PaginationRequest` (default 10, max 100) on all list endpoints — no unbounded queries
