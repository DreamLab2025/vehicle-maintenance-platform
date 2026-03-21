# Architecture Reference — .NET Clean Architecture

To apply Clean Architecture: 4 projects per service, strict layer boundaries, single direction of dependency.

**In this repo (Verendar):** project names are `Verendar.{Service}` (Host), `Verendar.{Service}.Application`, `Verendar.{Service}.Domain`, `Verendar.{Service}.Infrastructure`, e.g. `Verendar.Vehicle`, `Verendar.Vehicle.Application`.

---

## Layer Map & Dependency Rule

```
Host (Verendar.{Service})          ← Minimal API endpoints, Bootstrapping/, Program.cs
  └─ depends on → Application, Common, Infrastructure (composition root)

Application (Verendar.{Service}.Application)  ← Use cases, DTOs, Validators, Mappings
  └─ depends on → Domain, Common

Infrastructure (Verendar.{Service}.Infrastructure) ← EF Core DbContext, repositories, external services
  └─ depends on → Application, Domain, Common

Domain (Verendar.{Service}.Domain)  ← Entities, enums, repository interfaces — no external dependencies
Common (Verendar.Common)            ← ApiResponse<T>, Pagination, Middleware, ICacheService, ICurrentUserService
```

**The rule:** dependencies flow inward only. Infrastructure implements Domain repository interfaces and Application service interfaces. Application calls Domain interfaces. Api calls Application interfaces and wires Infrastructure in the composition root. Nothing points outward.

---

## Layer Responsibilities

### Domain — What exists

- Entities (state only, no behavior beyond simple property logic)
- Repository interfaces (`IGenericRepository<T>`, `IUnitOfWork`, `IXxxRepository`)
- No EF Core, no DTOs, no business logic, no external references

**Entity base class** — always inherit from `BaseEntity`, never implement `IAuditableEntity` directly:

```csharp
// ✅ Correct — inherits Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
public class UserVehicle : BaseEntity
{
    public Guid UserId { get; set; }
    public string? LicensePlate { get; set; }
}

// ❌ Wrong — manually redeclaring fields that BaseEntity already provides
public class UserVehicle : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    // ...
}
```

`IAuditableEntity` / `ISoftDeleteEntity` are infrastructure contracts — only `PostgresRepository<T>` and `BaseDbContext` reference them directly.

### Application — What to do

- Services: orchestrate repositories + external services, return `ApiResponse<T>`
- DTOs: request/response shapes
- Validators: FluentValidation — Vietnamese user-facing messages
- Mappings: static extension methods (`ToResponse()`, `ToEntity()`) — no AutoMapper
- Services and validators registered in `Bootstrapping/ApplicationServiceExtensions.cs` via `AddApplicationServices()`

### Infrastructure — How to persist/communicate

- EF Core DbContext + configurations + migrations
- Repository implementations (inherit `PostgresRepository<T>`)
- UnitOfWork implementation
- External services (e.g. Gemini AI, RabbitMQ consumers)
- Seed data (development only, guarded by `IsDevelopment()`)

### Api — How to expose

- Endpoint classes: one static class per module, in `Apis/` (e.g. `UserVehicleApis.cs`)
- Pattern: `MapXxxApi()` (registers group) → `MapXxxRoutes()` (declares routes + metadata) → private handler methods
- `MapXxxApi()` called from `UseApplicationServices()` in `Bootstrapping/ApplicationServiceExtensions.cs`
- `Program.cs`: minimal — calls `AddApplicationServices()`, `MigrateDbContextAsync<T>()`, `UseApplicationServices()`

### Common — Shared plumbing

- `ApiResponse<T>` — uniform response wrapper; use `SuccessResponse`, `SuccessPagedResponse`, `FailureResponse`
- `PaginationRequest` (default 10, max 100), `PagingMetadata`
- `ValidationEndpointFilter.Validate<T>()` — FluentValidation filter for write endpoints
- `GlobalExceptionsMiddleware` — catches unhandled exceptions, returns RFC 7807 in dev / generic message in prod
- `ICurrentUserService` — extracts `UserId` from JWT; inject in handlers and `BaseDbContext`
- `ICacheService` — Redis wrapper; use `SetIfNotExistsAsync` for atomic SET NX
- `BaseDbContext` — sets `CreatedBy`/`UpdatedBy` from `ICurrentUserService` on `SaveChangesAsync`

---

## What Goes Where — Quick Reference

| Thing                      | Layer          | Location                                            |
| -------------------------- | -------------- | --------------------------------------------------- |
| Entity class               | Domain         | `Entities/`                                         |
| Domain constants           | Domain         | `Constants/{Name}Constants.cs`                      |
| Repository interface       | Domain         | `Repositories/I{Name}Repository.cs`                 |
| IUnitOfWork property       | Domain         | `Repositories/IUnitOfWork.cs`                       |
| Service interface          | Application    | `Services/Interfaces/I{Name}Service.cs`             |
| Service implementation     | Application    | `Services/Implements/{Name}Service.cs`              |
| Request/Response DTO       | Application    | `Dtos/{Module}/`                                    |
| FluentValidation validator | Application    | `Validators/{Name}Validator.cs`                     |
| Mapping extensions         | Application    | `Mappings/{Name}Mappings.cs`                        |
| Repository implementation  | Infrastructure | `Repositories/Implements/{Name}Repository.cs`       |
| DbContext                  | Infrastructure | `Data/{Service}DbContext.cs` (e.g. `VehicleDbContext`) |
| External service           | Infrastructure | `ExternalServices/`                                 |
| Endpoint class             | Host           | `Apis/{Name}Apis.cs`                                |
| DI registration            | Host           | `Bootstrapping/ApplicationServiceExtensions.cs`     |

---

## Boundary Rules

**Domain must NOT contain:** DTOs, EF attributes (except `[Table]`/`[Column]`), HTTP types, business logic in entities, references to Application or Infrastructure.

**Application must NOT contain:** DbContext, EF queries, HttpContext, endpoint registration, references to Infrastructure or Api.

**Infrastructure must NOT contain:** Business rules, DTOs, endpoint logic — only persistence and external I/O.

**Api must NOT contain:** Business logic, direct DbContext usage, complex data transformations — delegate to Application.

---

## Checklist: Adding a New Feature

```
Domain
[ ] Entity in Domain/Entities/ — inherit BaseEntity
[ ] Repository interface in Domain/Repositories/Interfaces/ — extend IGenericRepository<T>
[ ] IUnitOfWork extended with new repo property

Application
[ ] Service interface in Services/Interfaces/ — return ApiResponse<T>
[ ] Service implementation in Services/Implements/ — try/catch, Vietnamese messages, ownership by userId
[ ] DTOs in Dtos/{Module}/ — separate XxxRequest / XxxResponse; filter inherits PaginationRequest
[ ] Mapping extensions in Mappings/{Name}Mappings.cs — ToEntity(), ToResponse(), UpdateEntity()
[ ] Validator in Validators/{Name}Validator.cs — structural rules only, Vietnamese messages
[ ] Service registered as Scoped in Bootstrapping/ApplicationServiceExtensions.cs

Infrastructure
[ ] Repository implementation in Repositories/Implements/ — inherit PostgresRepository<T>
[ ] DbSet added to Data/{Service}DbContext.cs
[ ] Partial indexes with HasFilter("\"DeletedAt\" IS NULL") in OnModelCreating
[ ] Global query filter HasQueryFilter(e => e.DeletedAt == null) in OnModelCreating
[ ] UnitOfWork property added
[ ] Migration: dotnet ef migrations add <Name> --project Verendar.{Service}.Infrastructure

Host
[ ] Endpoint class in Apis/{Name}Apis.cs — MapXxxApi() + MapXxxRoutes() + private handlers
[ ] Write routes: AddEndpointFilter(ValidationEndpointFilter.Validate<T>())
[ ] All non-public routes: .RequireAuthorization()
[ ] All handlers: extract userId → check Guid.Empty → call service → Results.*
[ ] WithOpenApi(op => { op.Summary = "Vietnamese summary"; return op; }) on every route
[ ] MapXxxApi() called in UseApplicationServices()
```
