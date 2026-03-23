# Code Quality Reference

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
