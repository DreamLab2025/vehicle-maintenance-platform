---
name: backend-development
description: >
  .NET Clean Architecture backend guide (Minimal API, EF Core, RBAC). Use when
  implementing features, designing APIs, writing or reviewing tests, reviewing
  code quality, or optimizing performance. References: api-design, architecture,
  solid, design-patterns, clean-code-refactoring, performance, testing. Repo: ResearchHub.
---

# Backend Development — .NET Clean Architecture

To apply standards, patterns, and conventions for .NET Clean Architecture backends (Minimal API, EF Core, RBAC). Load only the references needed for the task.

## References

| Situation                                          | When to load           | File |
| -------------------------------------------------- | ---------------------- | ---- |
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

Stack below is for this repository; adapt for other .NET backends.

| Concern      | Technology                                              |
| ------------ | ------------------------------------------------------- |
| Runtime      | .NET 10, C# 13                                          |
| API style    | Minimal API (no Controllers)                            |
| ORM          | EF Core + PostgreSQL                                    |
| Validation   | FluentValidation                                        |
| Auth         | JWT Bearer + RBAC (role_permissions table in this repo) |
| Logging      | Serilog — console (dev), Seq port 8888 (prod)           |
| Caching      | Redis (`ICacheService`)                                 |
| Email        | Resend API                                              |
| File storage | AWS S3 + CloudFront                                     |
| Password     | `IPasswordHasher` (ASP.NET Core Identity hasher)        |

---

## Solution Layout

Generic 5-project layout. **In this repo:** project names are ResearchHub.\* (e.g. ResearchHub.Api, ResearchHub.Domain).

```
src/
├── Domain/          → Entities, Repository interfaces
├── Application/     → Services, DTOs, Validators, Mappings, Constants/Messages (AppMessages, ValidatorMessages)
├── Infrastructure/  → EF Core, Repository implementations, External services
├── Common/           → ApiResponse, Pagination, Middleware, Extensions
└── Api/              → Minimal API endpoints, Bootstrapping
```

---

## Core Rules (Non-Negotiable)

1. **No AutoMapper** — static extension methods only (`ToResponse()`, `ToEntity()`, `ApplyUpdate()`)
2. **No MediatR / CQRS** — services call repositories directly via `IUnitOfWork`
3. **No Controllers** — Minimal API with `MapGroup` + `Map*Endpoints()` only
4. **No string literals for roles** — use `RoleConstants.X` (`Domain/Constants/RoleConstants.cs`) in all `RequireRole()` calls; never write `"admin"` inline
5. **Always async** — `async/await` throughout, never `.Result` or `.Wait()`
6. **Always soft delete** — set `DeletedAt = DateTime.UtcNow`, never call `DbContext.Remove()`
7. **Always paginate lists** — `PaginationRequest` + `GetPagedAsync`, never return unbounded lists

---

## Related

- **Refs:** See References table; load only what the task needs.
- **Other skills:** `code-review` for review feedback and verification gates. To update this skill: use `skill-creator` and apply format (frontmatter, References table, &lt;200 lines, imperative).
