# Code Quality Reference

## Do / Don't

### Service pattern
```csharp
// DO — primary constructor + private readonly fields + return ApiResponse<T>
public class BrandService(ILogger<BrandService> logger, IUnitOfWork unitOfWork) : IBrandService
{
    private readonly ILogger<BrandService> _logger     = logger;
    private readonly IUnitOfWork           _unitOfWork = unitOfWork;

    public async Task<ApiResponse<BrandResponse>> GetBrandByIdAsync(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null)
            return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy thương hiệu");
        return ApiResponse<BrandResponse>.SuccessResponse(brand.ToResponse(), "Thành công");
    }
}

// DON'T — throw exceptions for business failures, inject DbContext directly
public class BrandService
{
    private readonly VehicleDbContext _db;  // ❌ inject IUnitOfWork instead

    public async Task<BrandResponse> GetBrandByIdAsync(Guid id)
    {
        var brand = await _db.Brands.FindAsync(id);
        if (brand is null) throw new NotFoundException("Not found");  // ❌ return ApiResponse instead
        return brand.ToResponse();  // ❌ return the envelope, not the raw DTO
    }
}
```

### Mapping
```csharp
// DO — static extension methods in Application/Mappings/
public static BrandResponse ToResponse(this Brand b) => new() { Id = b.Id, Name = b.Name, ... };

// DON'T — AutoMapper or manual mapping inline in the service
services.AddAutoMapper(typeof(BrandProfile)); // ❌ no AutoMapper
var dto = new BrandResponse { Id = brand.Id, Name = brand.Name }; // ❌ inline in service body
```

### FluentValidation scope
```csharp
// DO — structural rules only in the validator
RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

// DON'T — business rules in the validator
RuleFor(x => x.VehicleTypeId).MustAsync(async (id, _) =>
    await _unitOfWork.Types.GetByIdAsync(id) != null);  // ❌ business check → belongs in service
```

### MassTransit event publishing
```csharp
// DO — publish after SaveChanges, swallow publish failures
await _unitOfWork.SaveChangesAsync();
try { await _publishEndpoint.Publish(new BrandCreatedEvent { ... }); }
catch (Exception ex) { _logger.LogWarning(ex, "Publish failed"); }

// DON'T — publish before save, or let a publish failure roll back the request
await _publishEndpoint.Publish(new BrandCreatedEvent { ... }); // ❌ publish before save
await _unitOfWork.SaveChangesAsync();   // if publish throws above, save never runs
```

---


## Service Pattern

Primary constructor, `private readonly` fields, return `ApiResponse<T>`, business rules before repo.
Inject `IPublishEndpoint` only in services that publish MassTransit events (e.g., Vehicle — not Location).

```csharp
public class BrandService(
    ILogger<BrandService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IBrandService  // IPublishEndpoint: Vehicle-specific
{
    private readonly ILogger<BrandService> _logger          = logger;
    private readonly IUnitOfWork           _unitOfWork      = unitOfWork;
    private readonly IPublishEndpoint      _publishEndpoint = publishEndpoint;

    public async Task<ApiResponse<BrandResponse>> CreateBrandAsync(BrandRequest request)
    {
        // 1. Business rule checks first
        if (await BrandNameExistsAsync(request.Name))
            return ApiResponse<BrandResponse>.ConflictResponse("Thương hiệu đã tồn tại");

        var vehicleType = await _unitOfWork.Types.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
            return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy loại xe");

        // 2. Persist
        var brand = request.ToEntity();
        await _unitOfWork.Brands.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        // 3. Re-fetch with navigations, then return
        var created = await _unitOfWork.Brands.GetByIdWithTypesAsync(brand.Id);
        return ApiResponse<BrandResponse>.CreatedResponse(created!.ToResponse(), "Tạo thương hiệu thành công");
    }
}
```

## Mapping Extensions (No AutoMapper)

Static methods in `Application/Mappings/`. Navigation properties may be null — guard with `?.`.

```csharp
public static class BrandMappings
{
    public static BrandResponse ToResponse(this Brand b) => new()
    {
        Id              = b.Id,
        VehicleTypeName = b.VehicleType?.Name ?? string.Empty,
        Name            = b.Name,
        // ...
    };

    public static Brand ToEntity(this BrandRequest r) => new()
    {
        Name          = r.Name,
        VehicleTypeId = r.VehicleTypeId,
        // ...
    };

    public static void UpdateEntity(this Brand b, BrandRequest r)
    {
        b.Name    = r.Name;
        b.Website = r.Website;
        // ...
    }
}
```

## DTOs

```csharp
// Filter request extends PaginationRequest
public class BrandFilterRequest : PaginationRequest { public Guid? TypeId { get; set; } }

// Separate list (summary) and detail (full) response types
public class BrandSummary { public Guid Id; public string Name; public string? LogoUrl; /* ... */ }
public class BrandResponse { public Guid Id; public string Name; public DateTime CreatedAt; /* ... */ }
```

## Repository Interface

Extend `IGenericRepository<T>` — provides `GetByIdAsync`, `AddAsync`, `DeleteAsync`, `FindOneAsync`, etc.
Add only entity-specific query methods.

```csharp
public interface IBrandRepository : IGenericRepository<Brand>
{
    IQueryable<Brand> AsQueryableWithVehicleType();
    Task<Brand?> GetByIdWithTypesAsync(Guid id);
}
```

## Soft Delete

`DeleteAsync(id)` sets `DeletedAt = DateTime.UtcNow`. Never `DbContext.Remove()`.

## FluentValidation

Structural rules only (format, length, not-null). Business rules stay in the service.

```csharp
public class BrandRequestValidator : AbstractValidator<BrandRequest>
{
    public BrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.VehicleTypeId).NotEmpty();
    }
}
// Register: builder.Services.AddValidatorsFromAssemblyContaining<BrandRequestValidator>();
```

## MassTransit Events

Publish after `SaveChangesAsync()`. Wrap in try/catch — publish failure should not fail the request.

```csharp
await _unitOfWork.SaveChangesAsync();
try { await _publishEndpoint.Publish(new BrandLogoMediaSupersededEvent { ... }); }
catch (Exception ex) { _logger.LogWarning(ex, "Failed to publish event {BrandId}", brandId); }
```
