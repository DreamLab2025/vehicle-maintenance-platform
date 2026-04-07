# API Design — Verendar / .NET Specific Patterns

> These patterns are **mandatory** in Verendar. They override or extend the general REST patterns.

## ApiResponse\<T\> — Mandatory Wrapper

All public endpoints must return `ApiResponse<T>`. This is a hard constraint from CLAUDE.md.

```csharp
// In {Module}Apis.cs — always return ApiResponse<T>
private static async Task<IResult> GetBookingHandler(
    Guid id, IBookingService service, CancellationToken ct)
{
    var result = await service.GetByIdAsync(id, ct);
    return result.IsSuccess
        ? Results.Ok(ApiResponse<BookingResponse>.Success(result.Data))
        : Results.NotFound(ApiResponse<BookingResponse>.Failure(result.Error));
}
```

## Internal Endpoints — Skip ApiResponse\<T\>

Endpoints under `/api/internal/...` are called by other services via `IServiceTokenProvider` auth.
They return plain objects (no `ApiResponse<T>` wrapper) and require `Role = "Service"`.

```csharp
// Internal-only group — no ApiResponse wrapper, authenticate as Service role
var internal = app.MapGroup("/api/internal/users")
    .RequireAuthorization(policy => policy.RequireRole("Service"));

internal.MapPost("/{userId}/roles", async (Guid userId, AssignRoleRequest req, IUserService svc, CancellationToken ct) =>
{
    var success = await svc.AssignRoleAsync(userId, req.Role, ct);
    return success ? Results.Ok() : Results.BadRequest();
});
```

## Pagination — PaginationRequest + GetPagedAsync

All list endpoints use `PaginationRequest` as `[AsParameters]` class (never separate `[FromQuery]` params):

```csharp
// Request DTO — always inherit PaginationRequest
public class GetBookingsRequest : PaginationRequest
{
    public BookingStatus? Status { get; set; }
    public Guid? GarageId { get; set; }
    public string? SortBy { get; set; }
}

// Endpoint
private static async Task<IResult> GetBookingsHandler(
    [AsParameters] GetBookingsRequest request,
    IBookingService service, CancellationToken ct)
{
    var result = await service.GetPagedAsync(request, ct);
    return Results.Ok(ApiResponse<PagedResult<BookingResponse>>.Success(result));
}
```

## Route Groups — {Module}Apis.cs

Each module registers its routes in a dedicated static class with private static handlers:

```csharp
// Garage/Verendar.Garage/Apis/BookingApis.cs
public static class BookingApis
{
    public static RouteGroupBuilder MapBookingApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/bookings")
            .RequireAuthorization()
            .WithTags("Bookings");

        group.MapGet("/", GetBookingsHandler);
        group.MapGet("/{id:guid}", GetBookingByIdHandler);
        group.MapPost("/", CreateBookingHandler)
             .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateBookingRequest>());
        group.MapPatch("/{id:guid}/cancel", CancelBookingHandler);

        return group;
    }

    private static async Task<IResult> GetBookingsHandler(...) { ... }
    // All handlers are private static methods
}
```

## Soft Delete — Never Hard Delete

```csharp
// CORRECT — soft delete
entity.DeletedAt = DateTime.UtcNow;
entity.DeletedBy = callerId;
await unitOfWork.SaveChangesAsync(ct);

// WRONG — never use this
await dbContext.Remove(entity);
```

## Error Mapping — RFC 7807

```csharp
// Map domain errors to RFC 7807 Problem Details
return Results.Problem(
    title: "Booking not found",
    detail: $"Booking {id} does not exist or has been deleted.",
    statusCode: 404,
    type: "https://tools.ietf.org/html/rfc7807");
```
