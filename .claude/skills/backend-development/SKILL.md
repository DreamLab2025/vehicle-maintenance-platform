---
name: backend-development
description: >
  .NET Clean Architecture backend guide (Minimal API, EF Core, RBAC). Use when
  implementing features, designing APIs, writing or reviewing tests, reviewing
  code quality, or optimizing performance. Repo: Verendar.
---

# Backend Development — .NET Clean Architecture

## References

| Need | File |
| ---- | ---- |
| API structure, routes, handlers, validation, pagination | `references/api-design.md` |
| Layer responsibilities, what goes where, add-feature checklist | `references/architecture.md` |
| SOLID, design patterns, clean code, refactoring | `references/code-quality.md` |
| N+1, caching, query hygiene, async patterns | `references/performance.md` |
| Unit / integration tests, coverage | `references/testing.md` |

Load only the references relevant to the current task.

---

## Stack

| Concern   | Technology |
| --------- | ---------- |
| Runtime   | .NET 9, Aspire 9.5 |
| API       | Minimal API (no Controllers) |
| ORM       | EF Core + PostgreSQL (one DB per service) |
| Auth      | JWT Bearer — `UserRole` enum on User entity |
| Messaging | RabbitMQ via MassTransit |
| Caching   | Redis — `ICacheService` |
| Background | Hangfire (Vehicle service only) |
| Gateway   | YARP |

---

## Core Rules

1. **No AutoMapper** — static extension methods: `ToEntity()`, `ToResponse()`, `UpdateFromRequest()`
2. **No MediatR / CQRS** — services call `IUnitOfWork` repositories directly
3. **No Controllers** — Minimal API with `MapGroup` + `MapXxxRoutes()` in `Bootstrapping/`
4. **No string role literals** — use `UserRole` enum in `RequireRole()`
5. **Always async** — `async/await` throughout, never `.Result` / `.Wait()`
6. **Always soft delete** — `DeletedAt = DateTime.UtcNow`, never `DbContext.Remove()`
7. **Always paginate** — `PaginationRequest` + `GetPagedAsync`, never unbounded lists
8. **No DB sharing** — cross-service data via HTTP (YARP) or RabbitMQ events
9. **Secrets via User Secrets** — never `appsettings.json`
