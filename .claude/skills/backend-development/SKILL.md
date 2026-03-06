---
name: backend-development
description: >
  Backend development mindset and patterns for .NET microservices. This skill should be used
  when making architectural decisions, designing APIs, structuring services, reviewing code quality,
  thinking about performance, or setting up infrastructure. Use it as a thinking framework —
  not a strict rulebook — to reason about tradeoffs in Clean Architecture, event-driven design,
  API contracts, SOLID principles, caching, DevOps, and cross-service communication.
  Grounded in the Verendar project but applicable as general backend engineering principles.
---

# Backend Development

A thinking guide for backend engineering. Each reference file is a lens — use them to reason through decisions, not to copy-paste solutions.

**The core habit**: Before writing code, ask *why* the current pattern exists, *whether* it applies here, and *what* you'd change to improve it.

---

## Reference Index

| File | Use When Thinking About |
|------|------------------------|
| `references/backend-architecture.md` | Service boundaries, layers, when to simplify or split, event vs HTTP, scaling structure |
| `references/backend-api-design.md` | Contract design, route shape, validation strategy, ApiResponse wrapper, versioning |
| `references/backend-code-quality.md` | SOLID tradeoffs, naming, mapping approaches, DI structure, recognizing code smells |
| `references/backend-devops.md` | Infrastructure setup, secrets, migrations, background jobs, running locally |
| `references/backend-mindset.md` | Design decisions, lazy vs eager, audit trails, security posture, when to break patterns |
| `references/backend-performance.md` | Query efficiency, caching strategy, async pitfalls, background job design, capacity signals |

---

## Principles to Internalize

These aren't rules — they're defaults worth questioning when context changes:

- **Domain-first thinking** — understand the business before picking the pattern
- **Prefer explicit over clever** — code that's obvious beats code that's "elegant"
- **Paginate by default** — unbounded queries are a future incident waiting to happen
- **Async by default** — blocking calls in async code are invisible until they're catastrophic
- **Secrets outside source** — one leaked key can undo months of work
- **One DB per service** — crossing DB boundaries couples services at the worst layer
- **Validate at the boundary** — trust internal types, distrust external input

Each of these has valid exceptions. The skill is knowing *when* to break them with intention.

---

## Current Project Context

**Stack**: .NET 9 · Aspire 9.5 · PostgreSQL · RabbitMQ · Redis · YARP · Hangfire

**Services**: Identity · Vehicle · Media · Notification · AI

**Architecture style**: Clean Architecture (Domain → Application → Infrastructure → Host) for domain services; monolithic for Identity (simpler, intentional).

**Key patterns in use**:
- Minimal API (`MapGroup` + `Map*Endpoints()`) instead of Controllers
- Repository + UnitOfWork for data access
- Static extension methods for DTO mapping (no AutoMapper)
- FluentValidation for request validation
- Bootstrapping extension methods for DI (not inline in Program.cs)
- User Secrets for all sensitive config

---

## Adding a New Feature — Thinking Checklist

Not a mechanical checklist — a sequence of questions:

1. **Which domain owns this?** Does it fit an existing service or need a new one?
2. **What's the core entity?** Define it in Domain before touching anything else.
3. **What does the API contract look like?** Design the endpoint shape before implementation.
4. **What can go wrong?** Define validation rules and error states early.
5. **What's the data access pattern?** Read-heavy? Write-heavy? Needs caching?
6. **Any cross-service effects?** Event vs synchronous call — think about failure modes.
7. **What's the migration impact?** New table, new column, or just new logic?

---

## When to Deviate from Patterns

Patterns exist because they solved a problem. When context changes, the pattern may not fit:

- Small services can skip CA layers — a 3-endpoint service doesn't need 4 projects
- Async events add complexity — synchronous HTTP is fine when latency is acceptable
- Caching adds staleness risk — sometimes fresh data matters more than speed
- Soft deletes add query complexity — hard deletes are fine when data has no audit value

**Always document why** you deviated. The next engineer will thank you.
