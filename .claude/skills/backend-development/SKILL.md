---
name: backend-development
description: >
  .NET Clean Architecture backend guide for the Verendar repo (Minimal API, EF Core,
  PostgreSQL, microservices with Aspire). Always use this skill when touching any backend
  work: adding entities, services, validators, or endpoints; writing or fixing tests;
  reviewing architecture or code quality; fixing performance or N+1 issues; adding EF Core
  migrations; or any task involving layers, response contracts, AppMessages, or domain
  constants. When in doubt about where code goes or how to structure a feature in this
  codebase, consult this skill.
  References: api-design, architecture, code-quality, performance, testing, aspire.
---

# Backend Development — .NET Clean Architecture (Verendar)

To apply standards, patterns, and conventions for the Verendar microservices backend (Minimal API, EF Core, Aspire). Load only the references needed for the task.

## References

| Situation                                                            | When to load           | File                          |
| -------------------------------------------------------------------- | ---------------------- | ----------------------------- |
| New endpoint or API module                                           | API design, routes     | `references/api-design.md`    |
| Where code lives, layer structure                                    | Architecture decisions | `references/architecture.md`  |
| SOLID, patterns, clean code, refactoring                             | Code quality           | `references/code-quality.md`  |
| Slow queries, N+1, caching, pagination                               | Performance            | `references/performance.md`   |
| Unit tests, coverage                                                 | Testing                | `references/testing.md`       |
| Aspire orchestration, service registration, startup, database wiring | Aspire/infra           | `references/aspire.md`        |
| Inter-service HTTP clients, MassTransit events, consumers            | Service communication  | `references/communication.md` |

---

## Decision Tree

```
TASK?
│
├─ New feature / endpoint
│  ├─ Where does it live?          → references/architecture.md
│  ├─ How to structure the API?    → references/api-design.md
│  ├─ Service + validation code?   → references/code-quality.md
│  └─ Write tests for the service  → references/testing.md
│
├─ Code review / quality check
│  ├─ Architecture violations?     → references/architecture.md
│  ├─ API contract issues?         → references/api-design.md
│  └─ Code style / patterns?       → references/code-quality.md
│
├─ Performance issue
│  └─ Slow query / N+1 / memory    → references/performance.md
│
├─ Writing or reviewing tests
│  └─ Unit tests (xUnit + Moq)     → references/testing.md
│
├─ Aspire / infra
│  ├─ Adding a new service         → references/aspire.md
│  └─ Startup, DB wiring, Redis    → references/aspire.md
│
├─ Inter-service communication
│  ├─ HTTP client (service→service) → references/communication.md
│  └─ MassTransit publish/consume  → references/communication.md
│
└─ All of the above
   └─ Read relevant reference(s) before writing code
```

---

## Project Stack

| Concern       | Technology                                                                         |
| ------------- | ---------------------------------------------------------------------------------- |
| Runtime       | .NET 9, C# 13                                                                      |
| Orchestration | .NET Aspire 9.5                                                                    |
| API style     | Minimal API (no Controllers)                                                       |
| ORM           | EF Core + PostgreSQL                                                               |
| Validation    | FluentValidation (endpoint filter pattern)                                         |
| Auth          | JWT Bearer (`RequireAuthorization()`)                                              |
| Messaging     | RabbitMQ + MassTransit                                                             |
| Caching       | Redis (`ICacheService`)                                                            |
| Background    | Hangfire (dev-guarded dashboard)                                                   |
| Gateway       | YARP (port 8080)                                                                   |
| Task runner   | Taskfile (`task run`, `task build`, `task migrate:add NAME=X PROJECT=Y STARTUP=Z`) |

---

## Solution Layout

Each service is a **4-project slice** under `{Service}/`:

```
{Service}/
├── Verendar.{Service}              → Host: Minimal API endpoints, Program.cs, DI wiring
├── Verendar.{Service}.Application  → Services, DTOs, Validators, Mappings, Constants
├── Verendar.{Service}.Domain       → Entities, Repository interfaces, Enums
└── Verendar.{Service}.Infrastructure → EF Core, Repository implementations, DbContext
```

Services: `Identity`, `Vehicle`, `Media`, `Notification`, `Ai`, `Garage`, `Payment`, `Location`.

**Special cases:**

- `Verendar.Identity.Contracts` and `Verendar.Vehicle.Contracts` — shared MassTransit event contracts
- Identity service implementations live in `Infrastructure/Services/` (depend on ASP.NET Core Identity + MassTransit)
- `Verendar.Location.Tests` — reference implementation for unit tests

---

## Core Rules (Non-Negotiable)

1. **No AutoMapper** — static extension methods only (`ToResponse()`, `ToEntity()`)
2. **No MediatR / CQRS** — services call repositories directly via `IUnitOfWork`
3. **No Controllers** — Minimal API: `MapGroup` → `MapXxxRoutes()` → private handlers
4. **Always async** — `async/await` throughout, never `.Result` or `.Wait()`
5. **Always soft delete** — set `DeletedAt = DateTime.UtcNow`, never call `DbContext.Remove()`
6. **Always paginate lists** — `PaginationRequest` + `GetPagedAsync` (exception: Location uses unbounded lists — static reference data with Redis 24h TTL)
7. **Primary constructor** — `public class FooService(ILogger<FooService> logger, IUnitOfWork uow)` + `private readonly` fields
8. **Tests required** — every new API endpoint must have unit tests covering its service logic. When modifying existing logic, verify and update the relevant tests. When changing structure (entity fields, response shape, layer boundaries), update the test file to match. See `references/testing.md`.

---

## Related

- **Refs:** See References table; load only what the task needs.
- To update this skill: use `skill-creator` and apply format (frontmatter, References table, &lt;200 lines, imperative).
