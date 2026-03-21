# Verendar API Building Pattern

**Last Updated**: 2026-03-21
**Based On**: Vehicle Service Implementation
**Status**: Production Pattern

---

## Overview

This document defines the canonical pattern for building REST APIs in the Verendar platform using .NET 9 Minimal APIs with Clean Architecture.

**Key Principles**:
- No Controllers, no AutoMapper, no MediatR
- Minimal API with `MapGroup` + `MapXxxRoutes()` extensions
- Static mapping methods: `ToEntity()`, `ToResponse()`, `ToSummary()`, `UpdateEntity()`
- FluentValidation with Vietnamese messages
- `ApiResponse<T>` wrapper on all endpoints
- Primary constructors with readonly fields
- Always async/await, try-catch with logging

---

## Layer Breakdown

### 1. Domain Layer

**Entity Definition**:
```csharp
namespace Verendar.{Service}.Domain.Entities
{
    public class {Entity}
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        // Other properties...

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Soft delete
    }
}
```

**Repository Interface**:
```csharp
namespace Verendar.{Service}.Domain.Repositories.Interfaces
{
    public interface I{Entity}Repository : IBaseRepository<{Entity}>
    {
        // Custom query methods specific to this entity
        Task<{Entity}?> GetByNameAsync(string name);
        Task<List<{Entity}>> GetActiveAsync();
    }
}
```

**UnitOfWork Interface**:
```csharp
public interface IUnitOfWork : IBaseUnitOfWork
{
    I{Entity}Repository {Entities} { get; }
    I{AnotherEntity}Repository {Entities} { get; }
}
```

---

### 2. Application Layer

#### DTOs (in `/Dtos/{EntityName}Dtos.cs`)

```csharp
namespace Verendar.{Service}.Application.Dtos
{
    // For POST/PUT requests
    public class {Entity}Request
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        // Only writable properties
    }

    // For GET single entity (most detailed)
    public class {Entity}Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Includes relationships if loaded
    }

    // For GET list (minimal details)
    public class {Entity}Summary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        // Only essential fields for list display
    }

    // For GET with filtering
    public class {Entity}FilterRequest : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public Guid? ParentId { get; set; }
        // Filter-specific properties
    }
}
```

#### Validators (in `/Validators/{EntityName}RequestValidator.cs`)

```csharp
namespace Verendar.{Service}.Application.Validators
{
    public class {Entity}RequestValidator : AbstractValidator<{Entity}Request>
    {
        public {Entity}RequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên tối đa 100 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả tối đa 500 ký tự");
        }
    }
}
```

#### Mappings (in `/Mappings/{Service}Mappings.cs`)

```csharp
namespace Verendar.{Service}.Application.Mappings
{
    public static class {Service}Mappings
    {
        // Request → Entity
        public static {Entity} ToEntity(this {Entity}Request request)
        {
            return new {Entity}
            {
                Name = request.Name,
                Description = request.Description,
            };
        }

        // Entity → Response (detailed)
        public static {Entity}Response ToResponse(this {Entity} entity)
        {
            return new {Entity}Response
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        // Entity → Summary (list view)
        public static {Entity}Summary ToSummary(this {Entity} entity)
        {
            return new {Entity}Summary
            {
                Id = entity.Id,
                Name = entity.Name,
            };
        }

        // Update entity from request
        public static void UpdateEntity(this {Entity} entity, {Entity}Request request)
        {
            entity.Name = request.Name;
            entity.Description = request.Description;
        }
    }
}
```

#### Service Interface (in `/Services/Interfaces/I{Entity}Service.cs`)

```csharp
namespace Verendar.{Service}.Application.Services.Interfaces
{
    public interface I{Entity}Service
    {
        // List with pagination and filtering
        Task<ApiResponse<List<{Entity}Summary>>> GetAll{Entities}Async({Entity}FilterRequest request);

        // Get by ID
        Task<ApiResponse<{Entity}Response>> Get{Entity}ByIdAsync(Guid id);

        // Create
        Task<ApiResponse<{Entity}Response>> Create{Entity}Async({Entity}Request request);

        // Update
        Task<ApiResponse<{Entity}Response>> Update{Entity}Async(Guid id, {Entity}Request request);

        // Delete (soft delete)
        Task<ApiResponse<string>> Delete{Entity}Async(Guid id);
    }
}
```

#### Service Implementation (in `/Services/Implements/{Entity}Service.cs`)

```csharp
namespace Verendar.{Service}.Application.Services.Implements
{
    public class {Entity}Service(ILogger<{Entity}Service> logger, IUnitOfWork unitOfWork) : I{Entity}Service
    {
        private readonly ILogger<{Entity}Service> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<{Entity}Summary>>> GetAll{Entities}Async({Entity}FilterRequest request)
        {
            try
            {
                request.Normalize();
                IQueryable<{Entity}> query = _unitOfWork.{Entities}.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.Name.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                if (request.IsDescending.HasValue)
                {
                    query = request.IsDescending.Value
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt);
                }

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return ApiResponse<List<{Entity}Summary>>.SuccessPagedResponse(
                    items.Select(x => x.ToSummary()).ToList(),
                    totalCount,
                    request.PageNumber,
                    request.PageSize,
                    "Lấy danh sách thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {Entities}");
                return ApiResponse<List<{Entity}Summary>>.FailureResponse("Lỗi khi lấy danh sách");
            }
        }

        public async Task<ApiResponse<{Entity}Response>> Get{Entity}ByIdAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.{Entities}.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResponse<{Entity}Response>.NotFoundResponse("Không tìm thấy {entity}");
                }

                return ApiResponse<{Entity}Response>.SuccessResponse(
                    entity.ToResponse(),
                    "Lấy thông tin thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Entity} with ID: {Id}", id);
                return ApiResponse<{Entity}Response>.FailureResponse("Lỗi khi lấy thông tin");
            }
        }

        public async Task<ApiResponse<{Entity}Response>> Create{Entity}Async({Entity}Request request)
        {
            try
            {
                // Business rule validation
                if (await EntityNameExistsAsync(request.Name))
                {
                    return ApiResponse<{Entity}Response>.ConflictResponse("{Entity} đã tồn tại");
                }

                var entity = request.ToEntity();
                await _unitOfWork.{Entities}.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created {Entity} {Name} (ID: {Id})",
                    nameof({Entity}), entity.Name, entity.Id);

                return ApiResponse<{Entity}Response>.CreatedResponse(
                    entity.ToResponse(),
                    "Tạo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {Entity}");
                return ApiResponse<{Entity}Response>.FailureResponse("Lỗi khi tạo");
            }
        }

        public async Task<ApiResponse<{Entity}Response>> Update{Entity}Async(Guid id, {Entity}Request request)
        {
            try
            {
                var entity = await _unitOfWork.{Entities}.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResponse<{Entity}Response>.NotFoundResponse("Không tìm thấy");
                }

                // Business rule validation
                if (await EntityNameExistsAsync(request.Name, id))
                {
                    return ApiResponse<{Entity}Response>.ConflictResponse("Tên đã tồn tại");
                }

                entity.UpdateEntity(request);
                await _unitOfWork.{Entities}.UpdateAsync(id, entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated {Entity} {Name} (ID: {Id})",
                    nameof({Entity}), entity.Name, id);

                return ApiResponse<{Entity}Response>.SuccessResponse(
                    entity.ToResponse(),
                    "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Entity} with ID: {Id}", id);
                return ApiResponse<{Entity}Response>.FailureResponse("Lỗi khi cập nhật");
            }
        }

        public async Task<ApiResponse<string>> Delete{Entity}Async(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.{Entities}.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy");
                }

                await _unitOfWork.{Entities}.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted {Entity} {Name} (ID: {Id})",
                    nameof({Entity}), entity.Name, id);

                return ApiResponse<string>.SuccessResponse("Deleted", "Xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {Entity} with ID: {Id}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa");
            }
        }

        private async Task<bool> EntityNameExistsAsync(string name, Guid? excludeId = null)
        {
            var existing = await _unitOfWork.{Entities}
                .FindOneAsync(x => x.Name == name && x.DeletedAt == null);
            return existing != null && (!excludeId.HasValue || existing.Id != excludeId.Value);
        }
    }
}
```

---

### 3. Infrastructure Layer

#### Repository Implementation

```csharp
namespace Verendar.{Service}.Infrastructure.Repositories.Implements
{
    public class {Entity}Repository(
        {Service}DbContext context) : PostgresRepository<{Entity}>(context), I{Entity}Repository
    {
        public async Task<{Entity}?> GetByNameAsync(string name)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.Name == name && x.DeletedAt == null);
        }

        public async Task<List<{Entity}>> GetActiveAsync()
        {
            return await DbSet.Where(x => x.DeletedAt == null).ToListAsync();
        }
    }
}
```

#### UnitOfWork Implementation

```csharp
public class UnitOfWork({Service}DbContext context) : BaseUnitOfWork<{Service}DbContext>(context), IUnitOfWork
{
    private I{Entity}Repository? _{entity}s;
    private I{AnotherEntity}Repository? _{entities};

    public I{Entity}Repository {Entities} => _{entity}s ??= new {Entity}Repository(Context);
    public I{AnotherEntity}Repository {Entities} => _{entities} ??= new {AnotherEntity}Repository(Context);
}
```

---

### 4. Host/API Layer

#### API Endpoints (in `/Apis/{Entity}Apis.cs`)

```csharp
namespace Verendar.{Service}.Apis
{
    public static class {Entity}Apis
    {
        public static IEndpointRouteBuilder Map{Entity}Api(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/{entities}")
                .Map{Entity}Routes()
                .WithTags("{Entity} Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder Map{Entity}Routes(this RouteGroupBuilder group)
        {
            // GET all with pagination and filtering
            group.MapGet("/", GetAll{Entities})
                .WithName("GetAll{Entities}")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách {entities}";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<{Entity}Summary>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<{Entity}Summary>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            // GET by ID
            group.MapGet("/{id:guid}", Get{Entity}ById)
                .WithName("Get{Entity}ById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin {entity} theo ID";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            // POST create
            group.MapPost("/", Create{Entity})
                .AddEndpointFilter(ValidationEndpointFilter.Validate<{Entity}Request>())
                .WithName("Create{Entity}")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới {entity} (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            // PUT update
            group.MapPut("/{id:guid}", Update{Entity})
                .AddEndpointFilter(ValidationEndpointFilter.Validate<{Entity}Request>())
                .WithName("Update{Entity}")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật {entity} (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<{Entity}Response>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            // DELETE
            group.MapDelete("/{id:guid}", Delete{Entity})
                .WithName("Delete{Entity}")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa {entity} (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetAll{Entities}([AsParameters] {Entity}FilterRequest request, I{Entity}Service service)
        {
            var result = await service.GetAll{Entities}Async(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Get{Entity}ById(Guid id, I{Entity}Service service)
        {
            var result = await service.Get{Entity}ByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Create{Entity}({Entity}Request request, I{Entity}Service service)
        {
            var result = await service.Create{Entity}Async(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Update{Entity}(Guid id, {Entity}Request request, I{Entity}Service service)
        {
            var result = await service.Update{Entity}Async(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Delete{Entity}(Guid id, I{Entity}Service service)
        {
            var result = await service.Delete{Entity}Async(id);
            return result.ToHttpResult();
        }
    }
}
```

#### Bootstrapping (in `/Bootstrapping/ApplicationServiceExtensions.cs`)

Add to `AddApplicationServices()`:
```csharp
// Services
builder.Services.AddScoped<I{Entity}Service, {Entity}Service>();
// ... other services ...

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<{Entity}RequestValidator>();
```

Add to `UseApplicationServices()`:
```csharp
// In MapXxxApi() section:
app.Map{Entity}Api();
```

---

## DI Registration Order

In `ApplicationServiceExtensions.AddApplicationServices()`:

1. Database + caching
2. Background jobs (if any)
3. HTTP clients for external services
4. UnitOfWork
5. All domain services
6. All validators
7. Return builder

In `ApplicationServiceExtensions.UseApplicationServices()`:

1. `MapDefaultEndpoints()`
2. `UseCommonService()`
3. Hangfire dashboard (if development)
4. Hangfire recurring jobs
5. `MapOpenApi()` (if development)
6. `UseHttpsRedirection()`
7. All API mappings
8. Return app

---

## Status Codes Reference

| Scenario | Code | Response Type |
|----------|------|---------------|
| Success list GET | 200 | `SuccessPagedResponse` |
| Success single GET | 200 | `SuccessResponse` |
| Success POST | 201 | `CreatedResponse` |
| Success PUT | 200 | `SuccessResponse` |
| Success DELETE | 200 | `SuccessResponse` |
| Validation error | 400 | Handled by ValidationEndpointFilter |
| Authorization error | 401 | Framework handled |
| Not found | 404 | `NotFoundResponse` |
| Conflict (duplicate) | 409 | `ConflictResponse` |
| Server error | 500 | Handled by global exception middleware |

---

## Key Patterns

### 1. Always Use Primary Constructor + Readonly Fields
```csharp
public class {Entity}Service(ILogger<{Entity}Service> logger, IUnitOfWork unitOfWork) : I{Entity}Service
{
    private readonly ILogger<{Entity}Service> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
}
```

### 2. Try-Catch-Log Pattern
```csharp
try
{
    // Business logic
    return ApiResponse<T>.SuccessResponse(data, "Vietnamese message");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context {Context}", contextData);
    return ApiResponse<T>.FailureResponse("Lỗi khi thực hiện hành động");
}
```

### 3. Business Rule Validation Before DB Operation
```csharp
// Validate business rules BEFORE database operations
if (await EntityNameExistsAsync(request.Name))
{
    return ApiResponse<T>.ConflictResponse("Entity đã tồn tại");
}

// Then proceed with database operation
var entity = request.ToEntity();
await _unitOfWork.{Entities}.AddAsync(entity);
```

### 4. Pagination with Filtering
```csharp
request.Normalize(); // Normalize page number/size
IQueryable<T> query = _unitOfWork.{Entities}.AsQueryable();

// Apply filters
if (!string.IsNullOrWhiteSpace(request.SearchTerm))
{
    query = query.Where(x => x.Name.Contains(request.SearchTerm));
}

var totalCount = await query.CountAsync();

// Sort
query = request.IsDescending.HasValue && request.IsDescending.Value
    ? query.OrderByDescending(x => x.CreatedAt)
    : query.OrderBy(x => x.CreatedAt);

// Paginate
var items = await query
    .Skip((request.PageNumber - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync();

return ApiResponse<List<T>>.SuccessPagedResponse(
    items.Select(x => x.ToSummary()).ToList(),
    totalCount,
    request.PageNumber,
    request.PageSize,
    "Lấy danh sách thành công");
```

### 5. Soft Delete Pattern
```csharp
// In repository (inherited from BaseRepository)
await _unitOfWork.{Entities}.DeleteAsync(id); // Sets DeletedAt
await _unitOfWork.SaveChangesAsync();

// In queries (automatic via global query filter)
query.Where(x => x.DeletedAt == null) // Applied by DbContext automatically
```

### 6. Authorization on Endpoints
```csharp
// Public endpoints
.RequireAuthorization()

// Admin-only endpoints
.RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))

// Specific role
.RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Manager)))
```

---

## Vietnamese Message Conventions

- Validation errors: "Tên không được để trống" (Field không được để trống)
- Success: "Lấy danh sách thành công" / "Tạo thành công" / "Cập nhật thành công"
- Errors: "Lỗi khi lấy danh sách" / "Lỗi khi tạo"
- Not found: "Không tìm thấy {entity}"
- Conflict: "{Entity} đã tồn tại"

---

## Checklist: Creating a New API Endpoint

- [ ] **Domain**: Entity + Repository Interface + UnitOfWork Interface
- [ ] **Infrastructure**: Repository Implementation + UnitOfWork Implementation + DbContext configuration
- [ ] **Application DTOs**: Request, Response, Summary, FilterRequest
- [ ] **Application Validators**: {Entity}RequestValidator with FluentValidation
- [ ] **Application Mappings**: ToEntity(), ToResponse(), ToSummary(), UpdateEntity()
- [ ] **Application Service Interface**: I{Entity}Service with CRUD methods
- [ ] **Application Service Implementation**: {Entity}Service with try-catch, logging, business logic
- [ ] **API Endpoints**: {Entity}Apis.cs with MapGroup, status codes, authorization
- [ ] **Bootstrapping**: Register service in DI, register validator, register API endpoint
- [ ] **Global Usings**: Add necessary namespaces if new assembly
- [ ] **Tests**: Unit tests for service logic, integration tests for endpoints
- [ ] **Database Migration**: `dotnet ef migrations add {Name}`

---

## Reference Files

- **Example API**: `Vehicle/Verendar.Vehicle/Apis/BrandApis.cs`
- **Example Service**: `Vehicle/Verendar.Vehicle.Application/Services/Implements/BrandService.cs`
- **Example DTOs**: `Vehicle/Verendar.Vehicle.Application/Dtos/BrandDtos.cs`
- **Example Validator**: `Vehicle/Verendar.Vehicle.Application/Validators/BrandRequestValidator.cs`
- **Bootstrapping**: `Vehicle/Verendar.Vehicle/Bootstrapping/ApplicationServiceExtensions.cs`
