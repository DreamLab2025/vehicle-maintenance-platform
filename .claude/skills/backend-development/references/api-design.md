# API Design Reference — .NET Minimal API

Minimal API conventions, auth patterns, validation flow, and response contract for .NET backends. Examples use generic names.

---

## Endpoint structure

One static class per module: two public extension methods and private handlers.

```csharp
public static class ProjectEndpoints
{
    // Called from Program.cs
    public static IEndpointRouteBuilder MapProjectApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/projects")
            .MapProjectRoutes()
            .WithTags("Projects")
            .RequireRateLimiting("Fixed");
        return builder;
    }

    // Route declarations — metadata only, no logic
    public static RouteGroupBuilder MapProjectRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync)
            .WithName("GetAllProjects")
            .RequireAuthorization(p => p.RequireRole(RoleConstants.Admin))
            .Produces<ApiResponse<List<ProjectResponse>>>(200);

        group.MapPost("/", CreateAsync)
            .WithName("CreateProject")
            .RequireAuthorization(p => p.RequireRole(RoleConstants.LabLead, RoleConstants.CampusViceDirector))
            .Produces<ApiResponse<ProjectResponse>>(200)
            .Produces<ApiResponse<object>>(400);

        return group;
    }

    // Handlers — private, stateless, thin (validate → call service → return)
    private static async Task<IResult> CreateAsync(
        [FromBody] CreateProjectRequest request,
        [FromServices] IProjectService service,
        [FromServices] IValidator<CreateProjectRequest> validator,
        CancellationToken ct = default)
    {
        if (!request.ValidateRequest(validator, out var validationResult)) return validationResult!;
        return (await service.CreateAsync(request, ct)).ToHttpResult();
    }
}
```

---

## REST route summary

| Operation    | Method | Pattern                                  |
| ------------ | ------ | ---------------------------------------- |
| List (paged) | GET    | `/api/v1/{resources}`                    |
| Get one      | GET    | `/api/v1/{resources}/{id:guid}`          |
| Create       | POST   | `/api/v1/{resources}`                    |
| Update       | PUT    | `/api/v1/{resources}/{id:guid}`          |
| Delete       | DELETE | `/api/v1/{resources}/{id:guid}`          |
| Sub-action   | POST   | `/api/v1/{resources}/{id:guid}/{action}` |

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

Role constants live in `Domain/Constants/RoleConstants.cs`. Class: `RoleConstants`.

```csharp
// Single role
.RequireAuthorization(p => p.RequireRole(RoleConstants.Admin))

// Multiple roles (any match)
.RequireAuthorization(p => p.RequireRole(RoleConstants.LabLead, RoleConstants.CampusViceDirector))
```

**Resource ownership** is checked inside the service, not on the route. Pass `currentUser.UserId` into the service method when needed.

---

## Validation flow

```csharp
// 1. FluentValidation checks structure (required, maxlength, format)
if (!request.ValidateRequest(validator, out var validationResult)) return validationResult!;
// On failure → 400 { code: "VALIDATION_ERROR", metadata: { errors: [{ field, message, code }] } }

// 2. Service checks business rules (duplicates, FK existence, ownership)
var response = await service.CreateAsync(request, ct);
return response.ToHttpResult();
// On failure → ApiResponse with appropriate statusCode and AppMessages constant
```

Never put business validation in validators. Never put structural validation in services.

---

## Pagination

```csharp
// Endpoint normalizes inputs
var (page, size) = Normalize.Pagination(
    pagination.PageNumber ?? 1,
    pagination.PageSize   ?? PaginationConstants.DefaultPageSize,
    PaginationConstants.DefaultPageSize,   // 10
    PaginationConstants.MaxPageSize);       // 100

var response = await service.GetPagedAsync(page, size, ct);
return response.ToHttpResult();
```

All list endpoints must paginate. Never return unbounded lists.

**Search / filter params — inherit `PaginationRequest`:**

When a list endpoint needs filter or search params, define a dedicated request class in `Application/Dtos/<Domain>/` that inherits `PaginationRequest` and adds the extra query fields. Use `[AsParameters]` on the derived class in the handler — do **not** mix `[AsParameters] PaginationRequest` with separate `[FromQuery]` filter params.

```csharp
// Application/Dtos/User/UserListRequest.cs
public class UserListRequest : PaginationRequest
{
    public string? Search { get; set; }   // maps to ?search=
    public string? Role   { get; set; }   // maps to ?role=
}

// Endpoint handler
private static async Task<IResult> GetAllAsync(
    [AsParameters] UserListRequest req,
    [FromServices] IUserService svc,
    CancellationToken ct)
{
    var (page, size) = Normalize.Pagination(
        req.PageNumber ?? 1,
        req.PageSize   ?? PaginationConstants.DefaultPageSize,
        PaginationConstants.DefaultPageSize,
        PaginationConstants.MaxPageSize);

    return (await svc.GetPagedAsync(req.Search, req.Role, page, size, ct)).ToHttpResult();
}
```

> Use plain `[AsParameters] PaginationRequest` only when there are **no** filter/search params.

---

## Response contract

Every endpoint returns `ApiResponse<T>` via `.ToHttpResult()`. Shape:

```json
{ "isSuccess": true/false, "statusCode": 200, "code": null, "message": "...", "data": {...}, "metadata": null }
```

On failure, `code` is a machine-readable string from `AppMessages.X.Code` (e.g. `"USER_NOT_FOUND"`). On validation failure, `code` is `"VALIDATION_ERROR"` and `metadata` contains `{ "errors": [{ "field": "email", "message": "...", "code": "REQUIRED_EMAIL" }] }`.

**Message:** Always pass an explicit `message` for every response (success and failure) to improve UX; do not rely on the default. Examples: "Project created successfully.", "Faculty list retrieved successfully.", "Project not found."

Paginated metadata:

```json
"metadata": { "pageNumber": 1, "pageSize": 10, "totalItems": 45, "totalPages": 5, "hasNextPage": true }
```

**Status codes:**

| Situation                        | Code                             |
| -------------------------------- | -------------------------------- |
| Success                          | 200                              |
| Not found                        | 404                              |
| Validation / business rule       | 400                              |
| Unauthorized                     | 401                              |
| Forbidden (wrong role/ownership) | 403                              |
| Unhandled exception              | 500 (GlobalExceptionsMiddleware) |

---

## Handler signatures

```csharp
// GET list (no filters)
([FromServices] IXxxService svc, [AsParameters] PaginationRequest pg, CancellationToken ct)

// GET list with search/filter — inherit PaginationRequest
([AsParameters] XxxListRequest req, [FromServices] IXxxService svc, CancellationToken ct)

// GET by id
([FromRoute] Guid id, [FromServices] IXxxService svc, CancellationToken ct)

// POST / PUT (with validation)
([FromBody] XxxRequest req, [FromServices] IXxxService svc,
 [FromServices] IValidator<XxxRequest> v, CancellationToken ct)

// DELETE
([FromRoute] Guid id, [FromServices] IXxxService svc, CancellationToken ct)
```

---

## Rules

- No Controllers — Minimal API only
- URL depth: at most 3 levels after `/api/v1/`; if deeper, flatten
- `RequireAuthorization()` on every non-public route
- Handlers are thin: validate → delegate to service → return result
- Resource ownership verified inside service, not in endpoint
- Always use `ValidateRequest()` before calling service on write endpoints
- Always use `GetPagedAsync()` + `Normalize.Pagination()` for list endpoints
