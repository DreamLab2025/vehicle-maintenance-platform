# API Design Reference — .NET Minimal API

Minimal API conventions, auth patterns, validation flow, and response contract for .NET backends. Examples use generic names.

---

## Endpoint structure

One static class per module: two public extension methods and private handlers.

```csharp
public static class UserVehicleApis
{
    // 1. Register in UseApplicationServices() in Bootstrapping/ApplicationServiceExtensions.cs
    public static IEndpointRouteBuilder MapUserVehicleApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/user-vehicles")
            .MapUserVehicleRoutes()
            .WithTags("User Vehicle Api")
            .RequireRateLimiting("Fixed");
        return builder;
    }

    // 2. Route declarations — metadata only, no logic
    public static RouteGroupBuilder MapUserVehicleRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetUserVehicles)
            .WithName("GetUserVehicles")
            .WithOpenApi(op => { op.Summary = "Lấy danh sách xe"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<List<UserVehicleResponse>>>(200);

        group.MapGet("/{userVehicleId:guid}", GetUserVehicleById)
            .WithName("GetUserVehicleById")
            .WithOpenApi(op => { op.Summary = "Lấy chi tiết xe"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<UserVehicleResponse>>(200)
            .Produces<ApiResponse<UserVehicleResponse>>(404);

        group.MapPost("/", CreateUserVehicle)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UserVehicleRequest>())
            .WithName("CreateUserVehicle")
            .WithOpenApi(op => { op.Summary = "Thêm xe mới"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<UserVehicleResponse>>(200)
            .Produces<ApiResponse<UserVehicleResponse>>(400);

        group.MapPut("/{userVehicleId:guid}", UpdateUserVehicle)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UserVehicleRequest>())
            .WithName("UpdateUserVehicle")
            .WithOpenApi(op => { op.Summary = "Cập nhật thông tin xe"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<UserVehicleResponse>>(200)
            .Produces<ApiResponse<UserVehicleResponse>>(400);

        group.MapPatch("/{userVehicleId:guid}/odometer", UpdateOdometer)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateOdometerRequest>())
            .WithName("UpdateOdometer")
            .WithOpenApi(op => { op.Summary = "Cập nhật số km"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<UserVehicleResponse>>(200)
            .Produces<ApiResponse<UserVehicleResponse>>(400);

        group.MapDelete("/{userVehicleId:guid}", DeleteUserVehicle)
            .WithName("DeleteUserVehicle")
            .WithOpenApi(op => { op.Summary = "Xóa xe"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<string>>(200)
            .Produces<ApiResponse<string>>(400);

        return group;
    }

    // 3. Handlers — private, thin: extract userId → call service → return IResult
    private static async Task<IResult> GetUserVehicles(
        ICurrentUserService currentUserService,
        [AsParameters] PaginationRequest pagination,
        IUserVehicleService vehicleService)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Results.Unauthorized();

        var result = await vehicleService.GetUserVehiclesAsync(userId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> CreateUserVehicle(
        UserVehicleRequest request,
        ICurrentUserService currentUserService,
        IUserVehicleService vehicleService)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Results.Unauthorized();

        var result = await vehicleService.CreateUserVehicleAsync(userId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
```

---

## REST route summary

| Operation         | Method | Pattern                                  |
| ----------------- | ------ | ---------------------------------------- |
| List (paged)      | GET    | `/api/v1/{resources}`                    |
| Get one           | GET    | `/api/v1/{resources}/{id:guid}`          |
| Create            | POST   | `/api/v1/{resources}`                    |
| Full update       | PUT    | `/api/v1/{resources}/{id:guid}`          |
| Partial update    | PATCH  | `/api/v1/{resources}/{id:guid}/{field}`  |
| Delete            | DELETE | `/api/v1/{resources}/{id:guid}`          |
| Sub-action        | POST   | `/api/v1/{resources}/{id:guid}/{action}` |

---

## URL depth (max 3 levels)

Keep nested routes when depth ≤ 3 (e.g. under `/api/v1/projects/...`). When depth > 3, flatten under a separate top-level resource (e.g. `/api/v1/project-course/...`).

| Wrong (too deep)                                          | Correct (flattened)                                       |
| --------------------------------------------------------- | --------------------------------------------------------- |
| `POST .../courses/{projectCourseId}/questions` (4 levels) | `POST /api/v1/project-course/{projectCourseId}/questions` |
| `DELETE .../courses/{projectCourseId}` (4 levels)         | `DELETE /api/v1/project-course/{projectCourseId}`         |

In the service, accept the identifying id (e.g. `projectCourseId`) and resolve `projectId` / `projectMajorId` from the database; enforce authorization inside the service.

---

## Authorization

Verendar uses `UserRole` enum on the `User` entity. Most user-facing endpoints need no role restriction — just `.RequireAuthorization()`.

```csharp
// Any authenticated user
.RequireAuthorization()

// Admin only (where needed)
.RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)))
```

**Resource ownership** is checked inside the service by filtering queries with `v.UserId == userId`. Always extract `userId` in the handler and pass it to the service. Return `Results.Unauthorized()` if `userId == Guid.Empty`.

---

## Validation flow

```csharp
// 1. Structural validation — applied as endpoint filter, runs before handler
group.MapPost("/", CreateAsync)
    .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateRequest>());
// On failure → 400 RFC 7807 Problem Details { title, status, errors: { field: ["msg"] } }

// 2. Business rules — inside service, return ApiResponse<T>
var result = await service.CreateAsync(userId, request);
return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
```

Never put business validation in validators. Never put structural validation in services.

---

## Pagination

`PaginationRequest` (default 10, max 100) via `Normalize()` before use:

```csharp
// Service calls Normalize() at entry
paginationRequest.Normalize();

// Repository uses GetPagedAsync
var (items, totalCount) = await _unitOfWork.UserVehicles.GetPagedAsync(
    paginationRequest.PageNumber,
    paginationRequest.PageSize,
    filter: v => v.UserId == userId);

return ApiResponse<UserVehicleResponse>.SuccessPagedResponse(
    items.Select(v => v.ToResponse()).ToList(),
    totalCount,
    paginationRequest.PageNumber,
    paginationRequest.PageSize,
    "Lấy danh sách xe thành công");
```

All list endpoints must paginate. Never return unbounded lists.

**Search / filter params — inherit `PaginationRequest`:**

```csharp
// Application/Dtos/Vehicle/ModelFilterRequest.cs
public class ModelFilterRequest : PaginationRequest
{
    public Guid? BrandId { get; set; }
    public string? ModelName { get; set; }
}

// Endpoint handler — use [AsParameters]
private static async Task<IResult> GetAllModels(
    [AsParameters] ModelFilterRequest filter,
    IVehicleModelService svc)
{
    var result = await svc.GetAllModelsAsync(filter);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}
```

---

## Response contract

Every endpoint returns `ApiResponse<T>` from `Verendar.Common.Shared`. Shape:

```json
// Success (single)
{ "isSuccess": true, "message": "Thành công.", "data": {...}, "metadata": null }

// Success (paginated)
{ "isSuccess": true, "message": "...", "data": [...],
  "metadata": { "pageNumber": 1, "pageSize": 10, "totalItems": 45, "totalPages": 5, "hasNextPage": true } }

// Business failure
{ "isSuccess": false, "message": "Không tìm thấy xe.", "data": null, "metadata": null }
```

**Exception:** FluentValidation and unhandled 500s use RFC 7807 Problem Details (not ApiResponse).

**Message:** Always pass a Vietnamese message for user-facing responses.

**Status codes:**

| Situation                  | Handler return                            |
| -------------------------- | ----------------------------------------- |
| Success                    | `Results.Ok(result)`                      |
| Business failure           | `Results.BadRequest(result)`              |
| Not found                  | `Results.NotFound(result)`                |
| Unauthenticated            | `Results.Unauthorized()`                  |
| Unhandled exception        | 500 via `GlobalExceptionsMiddleware`      |

---

## Handler signatures

No `[FromBody]`/`[FromServices]`/`[FromRoute]` attributes needed — Minimal API binds implicitly:

```csharp
// GET list (no filters)
(ICurrentUserService currentUserService, [AsParameters] PaginationRequest pg, IXxxService svc)

// GET list with filters — inherit PaginationRequest
([AsParameters] XxxFilterRequest req, ICurrentUserService currentUserService, IXxxService svc)

// GET by id (ownership check inside service)
(Guid id, ICurrentUserService currentUserService, IXxxService svc)

// POST / PUT (validation via AddEndpointFilter on route declaration)
(XxxRequest request, ICurrentUserService currentUserService, IXxxService svc)

// DELETE
(Guid id, ICurrentUserService currentUserService, IXxxService svc)
```

---

## Rules

- No Controllers — Minimal API only
- URL depth: at most 3 levels after `/api/v1/`; if deeper, flatten
- `.RequireAuthorization()` on every non-public route
- `.WithOpenApi(op => { op.Summary = "Vietnamese summary"; return op; })` on every route
- `Produces<ApiResponse<T>>(statusCode)` — always typed, never `Produces<ApiResponse<object>>`
- Handlers are thin: extract userId → check `Guid.Empty` → call service → return `Results.*`
- Resource ownership verified inside service (filter queries by `userId`)
- Validation via `AddEndpointFilter(ValidationEndpointFilter.Validate<T>())` on write endpoints
- Pagination via `paginationRequest.Normalize()` + `GetPagedAsync()` in service
