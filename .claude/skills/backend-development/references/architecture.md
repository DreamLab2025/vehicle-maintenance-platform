# Architecture Reference — Verendar Microservices (.NET Clean Architecture)

To apply Clean Architecture within a Verendar service: 4 projects, strict layer boundaries, single direction of dependency.

**Project naming:** `Verendar.{Service}`, `Verendar.{Service}.Application`, `Verendar.{Service}.Domain`, `Verendar.{Service}.Infrastructure`.

---

## Layer Map & Dependency Rule

```
Verendar.{Service}              ← HTTP in/out, routing, auth, middleware, DI wiring
  └─ depends on → Application, Infrastructure (composition root)

Verendar.{Service}.Application  ← Business logic, use cases, DTOs, validators, mappings, constants
  └─ depends on → Domain

Verendar.{Service}.Infrastructure ← EF Core, DbContext, repository implementations, external services
  └─ depends on → Application, Domain

Verendar.{Service}.Domain       ← Entities, repository interfaces, enums — no external dependencies
```

**The rule:** dependencies flow inward only. Infrastructure implements Domain repository interfaces. Application calls Domain interfaces. Host calls Application interfaces and wires Infrastructure in the composition root. Nothing points outward.

---

## Layer Responsibilities

### Domain — What exists

- Entities (inherit `BaseEntity` — provides `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, audit fields)
- Repository interfaces (`IGenericRepository<T>`, `IUnitOfWork`, `IXxxRepository`)
- Enums, value objects
- No EF Core, no DTOs, no business logic, no external references

```csharp
// ✅ Inherit BaseEntity — never redeclare Id, CreatedAt, etc.
public class GarageBranch : BaseEntity
{
    public Guid GarageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;      // value object
    public Garage Garage { get; set; } = null!;        // navigation
}
```

### Application — What to do

- Services: primary constructor + `ILogger<T>` + `IUnitOfWork` (± `ICacheService`, external clients)
- Return `ApiResponse<T>` from every service method
- DTOs: request/response shapes
- Validators: FluentValidation structural rules
- Mappings: static extension methods (`ToEntity()`, `ToResponse()`)
- Constants: cache keys, app messages

**Service pattern:**

```csharp
public class GarageService(
    ILogger<GarageService> logger,
    IUnitOfWork unitOfWork) : IGarageService
{
    private readonly ILogger<GarageService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<GarageResponse>> CreateGarageAsync(
        Guid ownerId, GarageRequest request, CancellationToken ct = default)
    {
        var existing = await _unitOfWork.Garages.FindOneAsync(g => g.OwnerId == ownerId);
        if (existing != null)
            return ApiResponse<GarageResponse>.ConflictResponse("Tài khoản đã có garage đăng ký");

        var garage = request.ToEntity(ownerId);
        await _unitOfWork.Garages.AddAsync(garage);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<GarageResponse>.CreatedResponse(garage.ToResponse(), "Đăng ký garage thành công");
    }
}
```

### Infrastructure — How to persist/communicate

- EF Core `{Service}DbContext` (inherits `BaseDbContext`) + EF configurations + migrations
- Repository implementations (inherit `PostgresRepository<T>`)
- `UnitOfWork` implementation (lazy-initialized repo properties)
- External services, HTTP clients, MassTransit consumers

**Exception — Identity:** service implementations live in `Infrastructure/Services/` because they depend on `PasswordHasher<User>` and `IPublishEndpoint`.

### Host (Verendar.{Service}) — How to expose

- Endpoint classes: `{Module}Apis.cs` in `Apis/` — one class per module
- Pattern: `MapXxxApi` → `MapXxxRoutes` → private static handlers
- `Program.cs`: wire everything together (calls Add\* extensions from each layer)
- Hangfire dashboard and test user seeding are guarded by `IsDevelopment()`

---

## What Goes Where — Quick Reference

| Thing                      | Layer          | Location                                              |
| -------------------------- | -------------- | ----------------------------------------------------- |
| Entity class               | Domain         | `Entities/{Name}.cs`                                  |
| Enum                       | Domain         | `Enums/{Name}.cs`                                     |
| Repository interface       | Domain         | `Repositories/Interfaces/I{Name}Repository.cs`        |
| IUnitOfWork                | Domain         | `Repositories/Interfaces/IUnitOfWork.cs`              |
| Service interface          | Application    | `Services/Interfaces/I{Name}Service.cs`               |
| Service implementation     | Application    | `Services/Implements/{Name}Service.cs`                |
| Request/Response DTO       | Application    | `Dtos/{Name}Request.cs`, `Dtos/{Name}Response.cs`     |
| FluentValidation validator | Application    | `Validators/{Name}Validator.cs`                       |
| Mapping extensions         | Application    | `Mappings/{Name}MappingExtensions.cs`                 |
| Cache key constants        | Application    | `Shared/Const/CacheKeys.cs` (or similar)              |
| EF configuration           | Infrastructure | `Data/Configurations/{Name}Configuration.cs`          |
| Repository implementation  | Infrastructure | `Repositories/Implements/{Name}Repository.cs`         |
| UnitOfWork implementation  | Infrastructure | `Repositories/Implements/UnitOfWork.cs`               |
| DbContext                  | Infrastructure | `Data/{Service}DbContext.cs` (e.g. `GarageDbContext`) |
| Endpoint class             | Host           | `Apis/{Module}Apis.cs`                                |
| DI registration            | each layer     | `DependencyInjection/ServiceCollectionExtensions.cs`  |

---

## IUnitOfWork Pattern

```csharp
// Domain/Repositories/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IBaseUnitOfWork
{
    IGarageRepository Garages { get; }
    IGarageBranchRepository GarageBranches { get; }
    IGarageProductRepository GarageProducts { get; }
    IGarageMemberRepository Members { get; }
}

// Infrastructure/Repositories/Implements/UnitOfWork.cs
public class UnitOfWork(GarageDbContext context)
    : BaseUnitOfWork<GarageDbContext>(context), IUnitOfWork
{
    private IGarageRepository? _garages;
    public IGarageRepository Garages => _garages ??= new GarageRepository(Context);
    // ... other repos
}
```

---

## Boundary Rules

**Domain must NOT contain:** DTOs, EF attributes (except `[Index]`), HTTP types, references to Application or Infrastructure.

**Application must NOT contain:** DbContext, EF queries, HttpContext, endpoint registration, Infrastructure references.

**Infrastructure must NOT contain:** Business rules, DTOs, endpoint logic.

**Host must NOT contain:** Business logic, direct DbContext usage, complex data transformations.

---

## Checklist: Adding a New Feature

```
Domain
[ ] Entity in Domain/Entities/{Name}.cs (inherits BaseEntity)
[ ] Repository interface in Domain/Repositories/Interfaces/I{Name}Repository.cs
[ ] IUnitOfWork extended with new repo property
[ ] Enums in Domain/Enums/ if needed

Application
[ ] Service interface in Services/Interfaces/I{Name}Service.cs
[ ] Service implementation in Services/Implements/{Name}Service.cs
[ ] DTOs in Dtos/
[ ] Mapping extensions in Mappings/
[ ] Validator in Validators/
[ ] Service registered in DependencyInjection/ServiceCollectionExtensions.cs (Scoped)

Infrastructure
[ ] EF configuration in Data/Configurations/
[ ] Repository implementation in Repositories/Implements/{Name}Repository.cs
[ ] DbSet added to {Service}DbContext
[ ] UnitOfWork updated with new repo property + lazy field
[ ] Migration: task migrate:add NAME=AddFoo PROJECT={Service}/Verendar.{Service}.Infrastructure STARTUP={Service}/Verendar.{Service}

Host
[ ] Endpoint class in Apis/{Module}Apis.cs
[ ] Route group registered in Program.cs (app.Map{Module}Api())
```

---

## Migration Rules

**Use the task command — never create manually:**

```bash
task migrate:add NAME=InitialCreate PROJECT=Garage/Verendar.Garage.Infrastructure STARTUP=Garage/Verendar.Garage
task migrate:remove PROJECT=Garage/Verendar.Garage.Infrastructure STARTUP=Garage/Verendar.Garage
```

**Add migration immediately after changing the entity** — before any other entity changes. Commit the 3 generated files together: `.cs`, `.Designer.cs`, `ModelSnapshot.cs`.
