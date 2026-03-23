# API Design Reference

## Do / Don't

### Endpoint structure
```csharp
// DO — two static methods, private handlers
public static class BrandApis
{
    public static IEndpointRouteBuilder MapBrandApi(this IEndpointRouteBuilder builder) { ... }
    public static RouteGroupBuilder MapBrandRoutes(this RouteGroupBuilder group) { ... }
    private static async Task<IResult> GetBrandById(...) => (await service.GetBrandByIdAsync(id)).ToHttpResult();
}

// DON'T — controllers
[ApiController]
[Route("api/v1/brands")]
public class BrandController : ControllerBase { ... }   // ❌ no controllers ever
```

### Response envelope
```csharp
// DO
return ApiResponse<BrandResponse>.CreatedResponse(data, "Tạo thương hiệu thành công");

// DON'T
return Results.Ok(new { success = true, data = brand });   // ❌ skip the envelope
return Results.Created("/api/v1/brands/" + id, brand);     // ❌ bypass ToHttpResult()
```

### Validation
```csharp
// DO — endpoint filter; validator in Application layer
group.MapPost("/", CreateBrand)
    .AddEndpointFilter(ValidationEndpointFilter.Validate<BrandRequest>());

// DON'T — manual validation inside the handler
private static async Task<IResult> CreateBrand(BrandRequest req, IBrandService svc)
{
    if (string.IsNullOrEmpty(req.Name)) return Results.BadRequest(...); // ❌
    ...
}
```

### Internal endpoints
```csharp
// DO — return domain shape directly, no ApiResponse<T>
group.MapGet("/users/{id}", GetUserInternal);
private static async Task<IResult> GetUserInternal(Guid id, IUserService svc)
    => Results.Ok(await svc.GetInternalUserAsync(id));  // plain object

// DON'T — wrap internal responses in ApiResponse<T>
return ApiResponse<InternalUserDto>.SuccessResponse(...);  // ❌ internal endpoints skip the envelope
```

---

## Endpoint Structure

Two methods per resource: one creates the group, one registers routes. See `BrandApis.cs`.

```csharp
public static class BrandApis
{
    public static IEndpointRouteBuilder MapBrandApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/brands")
            .MapBrandRoutes()
            .WithTags("Brand Api")
            .RequireRateLimiting("Fixed");
        return builder;
    }

    public static RouteGroupBuilder MapBrandRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllBrands)
            .RequireAuthorization()
            .Produces<ApiResponse<List<BrandSummary>>>(200)
            .Produces(401);

        group.MapPost("/", CreateBrand)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<BrandRequest>())
            .RequireAuthorization(p => p.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<BrandResponse>>(201)
            .Produces<ApiResponse<BrandResponse>>(409)
            .Produces(401);

        // ... MapGet("/{id:guid}"), MapPut, MapDelete
        return group;
    }

    // Handlers — thin, always .ToHttpResult()
    private static async Task<IResult> GetBrandById(Guid id, IBrandService service)
        => (await service.GetBrandByIdAsync(id)).ToHttpResult();

    private static async Task<IResult> CreateBrand(BrandRequest request, IBrandService service)
        => (await service.CreateBrandAsync(request)).ToHttpResult();
}
```

## ToHttpResult()

Extension in `Verendar.Common.Shared` — maps `StatusCode` → `IResult`. Always use this, never `Results.Ok()` directly.

```csharp
response.StatusCode switch
{
    200 => Results.Ok(response),
    201 => Results.Json(response, statusCode: 201),
    400 => Results.BadRequest(response),
    404 => Results.NotFound(response),
    409 => Results.Conflict(response),
    _   => Results.Json(response, statusCode: response.StatusCode)
};
```

## Response Envelope

```csharp
ApiResponse<T>.SuccessResponse(data, "message")          // 200
ApiResponse<T>.CreatedResponse(data, "message")          // 201
ApiResponse<T>.NotFoundResponse("message")               // 404
ApiResponse<T>.ConflictResponse("message")               // 409
ApiResponse<T>.SuccessPagedResponse(items, total, page, size, "message")  // 200 + metadata
```

## Authorization

```csharp
// Group-level: all routes require JWT
builder.MapGroup("/api/v1/brands").RequireAuthorization();

// Route-level: admin only
group.MapPost("/", Create).RequireAuthorization(p => p.RequireRole(nameof(RoleType.Admin)));
```

Location endpoints omit `.RequireAuthorization()` — public reference data.

## Internal Endpoints

Routes under `/api/internal/` skip `ApiResponse<T>` and `.ToHttpResult()` — return domain shapes directly.

## Bootstrapping

Wire all groups in `UseApplicationServices`:
```csharp
app.MapBrandApi();
app.MapTypeApi();
// ...
```
