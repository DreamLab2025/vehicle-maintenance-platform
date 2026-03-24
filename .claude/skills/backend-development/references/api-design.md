# API Design Reference — Verendar Minimal API

Minimal API conventions, auth patterns, validation flow, and response contract for Verendar services.

---

## Endpoint structure

One static class per module: `{Module}Apis.cs`. Two public extension methods and private static handlers.

```csharp
public static class GarageBranchApis
{
    // Called from Program.cs
    public static IEndpointRouteBuilder MapGarageBranchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages")
            .MapGarageBranchRoutes()
            .WithTags("Garage Branch Api")
            .RequireRateLimiting("Fixed");
        return builder;
    }

    // Route declarations — metadata only, no logic
    public static RouteGroupBuilder MapGarageBranchRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/{garageId:guid}/branches", CreateBranch)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageBranchRequest>())
            .WithName("CreateGarageBranch")
            .WithOpenApi(op => { op.Summary = "Tạo chi nhánh mới cho garage"; return op; })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{garageId:guid}/branches", GetBranches)
            .WithName("GetGarageBranches")
            .RequireAuthorization()
            .Produces<ApiResponse<List<GarageBranchResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    // Handlers — private, stateless, thin (call service → return result)
    private static async Task<IResult> CreateBranch(
        [FromRoute] Guid garageId,
        [FromBody] GarageBranchRequest request,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Results.Unauthorized();

        var result = await branchService.CreateBranchAsync(garageId, userId, request);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBranches(
        [FromRoute] Guid garageId,
        IGarageBranchService branchService)
    {
        var result = await branchService.GetBranchesAsync(garageId);
        return result.ToHttpResult();
    }
}
```

**Register in Program.cs:**

```csharp
app.MapGarageApi();
app.MapGarageBranchApi();
```

---

## REST route summary

| Operation    | Method | Pattern                                    |
| ------------ | ------ | ------------------------------------------ |
| List (paged) | GET    | `/api/v1/{resources}`                      |
| Get one      | GET    | `/api/v1/{resources}/{id:guid}`            |
| Create       | POST   | `/api/v1/{resources}`                      |
| Update       | PUT    | `/api/v1/{resources}/{id:guid}`            |
| Delete       | DELETE | `/api/v1/{resources}/{id:guid}`            |
| Sub-resource | POST   | `/api/v1/{resources}/{id:guid}/{children}` |
| Action       | POST   | `/api/v1/{resources}/{id:guid}/{action}`   |

---

## URL depth (max 3 levels after `/api/v1/`)

Keep nested routes when depth ≤ 3. When depth > 3, flatten under a separate top-level resource.

| Wrong (too deep)                                      | Correct (flattened)                         |
| ----------------------------------------------------- | ------------------------------------------- |
| `POST /garages/{id}/branches/{bid}/products` (5 deep) | `POST /api/v1/garage-branch-products/{bid}` |

---

## Validation

Use `ValidationEndpointFilter` — no validator injection in handler signature.

```csharp
// Route: add filter declaratively
group.MapPost("/", Create)
    .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateRequest>());

// Handler: validation already done — just call service
private static async Task<IResult> Create(
    [FromBody] CreateRequest request,
    IMyService service)
{
    var result = await service.CreateAsync(request);
    return result.ToHttpResult();
}
```

**Two-tier validation:**

1. **FluentValidation** (`Validate<T>()` filter) — structural rules: required, length, format → 400
2. **Service** — business rules: duplicates, FK existence, ownership → `FailureResponse` with appropriate status

Never put business validation in validators. Never put structural validation in services.

---

## Authorization

```csharp
// All protected routes — JWT bearer required
.RequireAuthorization()

// Public routes — explicitly allow anonymous
.AllowAnonymous()
```

Resource ownership is enforced inside the service (not on the route). Get the current user via `ICurrentUserService`:

```csharp
private static async Task<IResult> MyHandler(
    ICurrentUserService currentUserService,
    IMyService service)
{
    var userId = currentUserService.UserId;
    if (userId == Guid.Empty) return Results.Unauthorized();
    // ...
}
```

---

## Pagination

```csharp
// Use GetPagedAsync — never GetAllAsync on large tables
var result = await _unitOfWork.Items.GetPagedAsync(
    pageNumber, pageSize,
    filter: i => i.DeletedAt == null,
    orderBy: q => q.OrderByDescending(i => i.CreatedAt),
    tracking: false);

// Return paginated response
return ApiResponse<List<ItemResponse>>.SuccessResponse(
    items.Select(i => i.ToResponse()).ToList(),
    "Lấy danh sách thành công",
    new PagingMetadata(pageNumber, pageSize, total));
```

**Exception:** Location service endpoints return unbounded lists (static reference data, Redis-cached 24h).

---

## Response contract

Every endpoint returns `ApiResponse<T>` via `.ToHttpResult()`. Shape:

```json
{ "isSuccess": true, "statusCode": 200, "message": "...", "data": {...}, "metadata": null }
```

**ApiResponse factory methods:**

```csharp
ApiResponse<T>.SuccessResponse(data, "Thành công")                     // 200
ApiResponse<T>.CreatedResponse(data, "Tạo thành công")                 // 201
ApiResponse<T>.NotFoundResponse("Không tìm thấy")                      // 404
ApiResponse<T>.ConflictResponse("Đã tồn tại")                          // 409
ApiResponse<T>.FailureResponse("Lỗi xử lý")                           // 400
```

**Messages:** Always provide a Vietnamese message string. Never rely on defaults.

**Status codes:**

| Situation                  | Code |
| -------------------------- | ---- |
| Success                    | 200  |
| Created                    | 201  |
| Not found                  | 404  |
| Validation / business rule | 400  |
| Conflict (duplicate)       | 409  |
| Unauthorized               | 401  |
| Unhandled exception        | 500  |

---

## Handler signatures

```csharp
// GET list (paged)
([AsParameters] PaginationRequest pg, IXxxService svc, CancellationToken ct)

// GET by id
([FromRoute] Guid id, IXxxService svc, CancellationToken ct)

// POST/PUT (body, validation handled by filter — no validator in handler)
([FromBody] XxxRequest req, ICurrentUserService cu, IXxxService svc, CancellationToken ct)

// DELETE
([FromRoute] Guid id, ICurrentUserService cu, IXxxService svc, CancellationToken ct)
```

---

## Rules

- No Controllers — Minimal API only
- URL depth: at most 3 levels after `/api/v1/`; if deeper, flatten
- `RequireAuthorization()` on every non-public route
- Handlers are thin: delegate to service → return result
- Validation via `ValidationEndpointFilter.Validate<T>()` — no validator parameter in handlers
- Resource ownership verified inside service, not in endpoint
- Always use `WithOpenApi()` with a Vietnamese summary
