# Architecture Reference

## Project Structure

```
Vehicle/
├── Verendar.Vehicle/              # Host — DI wiring, endpoint mapping
│   ├── Bootstrapping/ApplicationServiceExtensions.cs
│   └── Apis/BrandApis.cs, TypeApis.cs, ...
├── Verendar.Vehicle.Application/  # Services, DTOs, Mappings, Validators
│   ├── Dtos/BrandDtos.cs
│   ├── Mappings/BrandMappings.cs
│   ├── Services/Interfaces/IBrandService.cs
│   └── Services/Implements/BrandService.cs
├── Verendar.Vehicle.Domain/       # Entities, Repository interfaces, IUnitOfWork
│   ├── Entities/Brand.cs
│   └── Repositories/Interfaces/IBrandRepository.cs, IUnitOfWork.cs
└── Verendar.Vehicle.Infrastructure/ # EF Core, repo impls, UnitOfWork
    ├── Data/VehicleDbContext.cs
    └── Repositories/Implements/BrandRepository.cs, UnitOfWork.cs
```

- Application: no EF Core, no `IResult` — only `IUnitOfWork` and `ApiResponse<T>`
- Domain: no EF Core runtime dependencies (data annotations are fine)
- Infrastructure: extends `PostgresRepository<T>` and `BaseUnitOfWork<TContext>` from `Verendar.Common`

## IUnitOfWork

```csharp
// Domain — interface
public interface IUnitOfWork : IBaseUnitOfWork
{
    IBrandRepository  Brands  { get; }
    ITypeRepository   Types   { get; }
    IModelRepository  Models  { get; }
    IUserVehicleRepository UserVehicles { get; }
    // ...
}

// Infrastructure — lazy init
public class UnitOfWork(VehicleDbContext context)
    : BaseUnitOfWork<VehicleDbContext>(context), IUnitOfWork
{
    private IBrandRepository? _brands;
    public IBrandRepository Brands => _brands ??= new BrandRepository(Context);
    // ...
}
```

## Bootstrapping

```csharp
public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
{
    builder.AddServiceDefaults();
    builder.AddCommonService();
    builder.AddPostgresDatabase<VehicleDbContext>(Const.VehicleDatabase);

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IBrandService, BrandService>();
    // ...
    builder.Services.AddValidatorsFromAssemblyContaining<BrandRequestValidator>();
    return builder;
}

public static WebApplication UseApplicationServices(this WebApplication app)
{
    app.MapDefaultEndpoints();
    app.UseCommonService();

    if (app.Environment.IsDevelopment()) app.MapOpenApi();
    app.UseHttpsRedirection();

    app.MapBrandApi();
    app.MapTypeApi();
    // ...
    return app;
}
```

## Verendar.Common Base Classes

These are inherited/injected — don't reimplement them.

| Class / Interface | Package | What it gives you |
|---|---|---|
| `BaseEntity` | `Verendar.Common` | `Id` (Guid v7), `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By` |
| `BaseDbContext` | `Verendar.Common` | Auto-fills audit fields on save; global `DeletedAt == null` filter |
| `IGenericRepository<T>` | `Verendar.Common` | `GetByIdAsync`, `GetAllAsync`, `FindOneAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CountAsync`, `AsQueryable`, `GetPagedAsync` |
| `PostgresRepository<T>` | `Verendar.Common` | Implements `IGenericRepository<T>`; `DeleteAsync` soft-deletes if entity is `ISoftDeleteEntity` |
| `IBaseUnitOfWork` | `Verendar.Common` | `SaveChangesAsync`, `BeginTransactionAsync`, `CommitTransactionAsync`, `ExecuteInTransactionAsync` |
| `BaseUnitOfWork<TContext>` | `Verendar.Common` | Implements `IBaseUnitOfWork` with EF execution strategy |
| `ICacheService` | `Verendar.Common.Caching` | `GetAsync`, `SetAsync`, `SetIfNotExistsAsync`, `RemoveAsync` — keys auto-prefixed with service name |
| `AddCommonService()` | `Verendar.Common.Bootstrapping` | CORS, rate limiting, MassTransit/RabbitMQ, JWT auth, Swagger |
| `AddPostgresDatabase<T>()` | `Verendar.Common.Bootstrapping` | Aspire Postgres + EF Core + migrations assembly |
| `UseCommonService()` | `Verendar.Common.Bootstrapping` | Correlation ID, request logging, exception middleware, auth/authz, rate limiter |

Repo implementations extend `PostgresRepository<T>` and add only entity-specific queries:

```csharp
public class BrandRepository(VehicleDbContext context)
    : PostgresRepository<Brand>(context), IBrandRepository
{
    public IQueryable<Brand> AsQueryableWithVehicleType() =>
        _dbSet.Include(b => b.VehicleType);
}
```

## Entity Convention

Entities inherit `BaseEntity` (full audit + soft-delete fields — see table above).

```csharp
[Index(nameof(Slug), IsUnique = true)]
public class Brand : BaseEntity
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(50)]  public string Slug { get; set; } = string.Empty;
    public Guid VehicleTypeId { get; set; }

    // Navigation
    public VehicleType   VehicleType   { get; set; } = null!;
    public List<Model>   VehicleModels { get; set; } = [];
}
```

## New Service Checklist

- [ ] 4 projects: Host, Application, Domain, Infrastructure
- [ ] `ApplicationServiceExtensions` with `Add` + `Use` methods
- [ ] `IUnitOfWork` in Domain, `UnitOfWork` in Infrastructure
- [ ] `AddPostgresDatabase<TContext>()` with correct Aspire resource name
- [ ] Services registered, validators scanned
- [ ] Routes wired in `UseApplicationServices`
- [ ] AppHost: add service + DB dependency + gateway route
- [ ] `.sln` updated
