---
name: backend-development
description: >
  .NET Clean Architecture backend guide (Minimal API, EF Core, RBAC). Use when
  implementing features, designing APIs, writing or reviewing tests, reviewing
  code quality, or optimizing performance. References: api-design, architecture,
  solid, design-patterns, clean-code-refactoring, performance, testing. Repo: Verendar.
---

# Backend Development — .NET Clean Architecture

To apply standards, patterns, and conventions for .NET Clean Architecture backends (Minimal API, EF Core, JWT). Load only the references needed for the task.

## References

| Situation                                          | When to load           | File |
| -------------------------------------------------- | ---------------------- | ---- |
| Full working template for any layer                | Starting a new feature | `references/service-template.md` |
| New endpoint or API module                        | API design, routes     | `references/api-design.md` |
| Where code lives, layer structure                  | Architecture decisions | `references/architecture.md` |
| SOLID, patterns, clean code, refactoring           | Code quality           | `references/code-quality.md` |
| Slow queries, N+1, caching, pagination             | Performance            | `references/performance.md` |
| Unit/integration tests, coverage                   | Testing                | `references/testing.md` |

---

## Decision Tree

```
TASK?
│
├─ New feature / endpoint
│  ├─ Where does it live?          → references/architecture.md
│  ├─ How to structure the API?    → references/api-design.md
│  └─ Service + validation code?   → references/code-quality.md
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
│  └─ Unit, integration, E2E       → references/testing.md
│
└─ All of the above
   └─ Read relevant reference(s) before writing code
```

---

## Project Stack

| Concern        | Technology                                                         |
| -------------- | ------------------------------------------------------------------ |
| Runtime        | .NET 9, Aspire 9.5                                                 |
| API style      | Minimal API (no Controllers)                                       |
| ORM            | EF Core + PostgreSQL (one DB per service)                          |
| Validation     | FluentValidation — Vietnamese user-facing messages                 |
| Auth           | JWT Bearer — roles stored as `List<UserRole>` on the User entity   |
| Messaging      | RabbitMQ via MassTransit (async cross-service events)              |
| Caching        | Redis (`ICacheService`)                                            |
| Background     | Hangfire (Vehicle service)                                         |
| Reverse proxy  | YARP (gateway)                                                     |
| Password       | `PasswordHasher<User>` (ASP.NET Core Identity hasher)              |

---

## Solution Layout

Five services, each following Clean Architecture. **Project names:** `Verendar.{Service}` (e.g. `Verendar.Vehicle`, `Verendar.Identity.Application`).

```
Verendar/
├── App/
│   ├── Verendar.AppHost/          # Aspire orchestration
│   ├── Verendar.ServiceDefaults/  # Shared telemetry/health
│   └── Verendar.Common/           # ApiResponse, Pagination, JWT, Middleware, CacheService
├── {Service}/
│   ├── Verendar.{Service}/        # Host — Minimal API + Bootstrapping/
│   ├── Verendar.{Service}.Application/  # Use cases, DTOs, Validators, Mappings
│   ├── Verendar.{Service}.Domain/       # Entities, enums, repository interfaces
│   └── Verendar.{Service}.Infrastructure/ # EF Core, external services, repositories
```

Services: Identity, Vehicle, Media, Notification, Ai.

---

## Core Rules (Non-Negotiable)

1. **No AutoMapper** — static extension methods only (`ToResponse()`, `ToEntity()`)
2. **No MediatR / CQRS** — services call repositories directly via `IUnitOfWork`
3. **No Controllers** — Minimal API with `MapGroup` + `Map*Endpoints()` in `Bootstrapping/`
4. **No string literals for roles** — use `UserRole` enum in all `RequireRole()` calls
5. **Always async** — `async/await` throughout, never `.Result` or `.Wait()`
6. **Always soft delete** — set `DeletedAt = DateTime.UtcNow`, never call `DbContext.Remove()`
7. **Always paginate lists** — `PaginationRequest` + `GetPagedAsync`, never return unbounded lists
8. **No DB sharing** — cross-service data via HTTP (YARP) or RabbitMQ events only
9. **Secrets via User Secrets only** — never in `appsettings.json`

---

## Related

- **Refs:** See References table; load only what the task needs.
- **Other skills:** `code-review` for review feedback and verification gates. To update this skill: use `skill-creator` and apply format (frontmatter, References table, &lt;200 lines, imperative).
