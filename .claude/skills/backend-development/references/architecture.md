# Architecture Reference — .NET Clean Architecture

To apply Clean Architecture: 5 projects, strict layer boundaries, single direction of dependency.

**In this repo:** project names are ResearchHub.Api, ResearchHub.Application, ResearchHubDbContext, etc.

---

## Layer Map & Dependency Rule

```
Api              ← HTTP in/out, routing, auth, rate limiting
  └─ depends on → Application, Common, Infrastructure (composition root)

Application      ← Business logic, use cases, DTOs, validators, mappings
  └─ depends on → Domain, Common

Infrastructure   ← Data access (EF Core), external services (e.g. email, file storage)
  └─ depends on → Application, Domain, Common (implements Application service interfaces + Domain repo interfaces)

Domain           ← Entities, repository interfaces — no external dependencies
Common           ← Shared utilities (ApiResponse, Pagination, Middleware)
```

**The rule:** dependencies flow inward only. Infrastructure implements Domain repository interfaces and Application service interfaces. Application calls Domain interfaces. Api calls Application interfaces and wires Infrastructure in the composition root. Nothing points outward.

---

## Layer Responsibilities

### Domain — What exists

- Entities (state only, no behavior beyond simple property logic)
- Repository interfaces (`IGenericRepository<T>`, `IUnitOfWork`, `IXxxRepository`)
- No EF Core, no DTOs, no business logic, no external references

**Entity base class** — always inherit from `Entity`, never implement `IEntity` directly:

```csharp
// ✅ Correct — inherits Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
public class Project : Entity
{
    public string Name { get; set; } = null!;
}

// ❌ Wrong — manually redeclaring fields that Entity already provides
public class Project : IEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    // ...
}
```

`IEntity` / `IAuditableEntity` / `ISoftDeleteEntity` are infrastructure contracts — only `GenericRepository<T>` and `DbContext` configurations reference them directly.

### Application — What to do

- Services: orchestrate repositories + external services, return `ApiResponse<T>`
- DTOs: request/response shapes
- Validators: structural rules (FluentValidation) — always use `WithValidatorMessage(ValidatorMessages.X)`, never bare `.WithErrorCode` + `.WithMessage`
- Mappings: static extension methods (`ToResponse`, `ToEntity`, `ApplyUpdate`)
- Constants/Messages: `AppMessages` (`AppMessage` instances for `FailureResponse`), `ValidatorMessages` (`ValidatorMessage`/`ValidatorMessageFormat` for validators)
- Application services registered in `Application/DependencyInjection/ServiceCollectionExtensions.cs` (AddApplicationServices)

### Infrastructure — How to persist/communicate

- EF Core DbContext + configurations + migrations
- Repository implementations (inherit `GenericRepository<T>`)
- UnitOfWork implementation
- External services (e.g. email, file storage — in this repo: Resend, S3)
- Seed data

### Api — How to expose

- Endpoint classes: one static class per module (`ProjectEndpoints`)
- Pattern: `MapGroup → MapXxxRoutes → private handler methods`
- Bootstrapping: calls Add* extensions (AddPersistence, AddApplicationLayer, AddApiServices, etc.); middleware pipeline
- `Program.cs`: wire everything together (no direct service registrations; each layer registers in its own DependencyInjection)

### Common — Shared plumbing

- `ApiResponse<T>` + `ToHttpResult()` extension
- `PaginationRequest`, `PagingMetadata`, `PaginationConstants`
- `ValidationExtensions.ValidateRequest()`
- `GlobalExceptionsMiddleware`
- `ICurrentUserService`, `ICacheService`, `IPasswordHasher`
- `Normalize` (email, pagination clamping)

---

## What Goes Where — Quick Reference

| Thing                      | Layer          | Location                                            |
| -------------------------- | -------------- | --------------------------------------------------- |
| Entity class               | Domain         | `Entities/`                                         |
| Domain constants           | Domain         | `Constants/{Name}Constants.cs`                      |
| Repository interface       | Domain         | `Repositories/I{Name}Repository.cs`                 |
| IUnitOfWork property       | Domain         | `Repositories/IUnitOfWork.cs`                       |
| Service interface          | Application    | `Services/Interfaces/I{Name}Service.cs`             |
| Service implementation     | Application    | `Services/Implementations/{Name}Service.cs`         |
| Request/Response DTO       | Application    | `Dtos/{Module}/`                                    |
| FluentValidation validator | Application    | `Validators/{Module}/`                              |
| Mapping extensions         | Application    | `Mappings/{Name}MappingExtensions.cs`               |
| EF configuration           | Infrastructure | `Persistence/Configurations/{Name}Configuration.cs` |
| Repository implementation  | Infrastructure | `Repositories/{Name}Repository.cs`                  |
| DbContext                  | Infrastructure | `Persistence/{Project}DbContext.cs` (this repo: ResearchHubDbContext) |
| External service           | Infrastructure | `ExternalServices/`                                 |
| Endpoint class             | Api            | `Apis/{Module}Endpoints.cs`                         |
| Application service registration | Application | `DependencyInjection/ServiceCollectionExtensions.cs` (AddApplicationServices) |
| Api bootstrapping (calls Add*) | Api        | `Bootstrapping/` (Program.cs, ApplicationServiceExtensions, etc.) |

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
[ ] Entity in Domain/Entities/
[ ] Repository interface in Domain/Repositories/
[ ] IUnitOfWork extended with new repo property
[ ] If new Entities/ sub-namespace: update GlobalUsings.cs in Domain, Application, Infrastructure

Application
[ ] Service interface + implementation in Services/
[ ] DTOs in Dtos/{Module}/
[ ] Mapping extensions in Mappings/
[ ] Validator in Validators/{Module}/
[ ] Service registered in ServiceCollectionExtensions (Scoped)

Infrastructure
[ ] EF configuration in Persistence/Configurations/
[ ] Repository implementation in Repositories/
[ ] DbSet added to DbContext (this repo: ResearchHubDbContext)
[ ] UnitOfWork updated
[ ] Migration: dotnet ef migrations add <Name>

Api
[ ] Endpoint class in Apis/
[ ] Group registered in Program.cs
```
