Implement the following feature for the Verendar .NET backend: $ARGUMENTS

Follow these conventions strictly:
- Clean Architecture: start in Domain (entity/value object), then Application (use case, validator, DTO), then Infrastructure (repo, EF config), then Host (Minimal API endpoint)
- Minimal API only — never Controllers. Use `MapGroup` + `Map*Endpoints()` in `Bootstrapping/`
- Static extension methods for DTO mapping (`ToResponse()`, `ToEntity()`) — no AutoMapper
- FluentValidation for all request DTOs, Vietnamese error messages for user-facing fields
- All list endpoints must be paginated with `PaginationRequest` (default 10, max 100)
- Wrap all responses in `ApiResponse<T>` from `App/Verendar.Common/Shared/ApiResponse.cs`
- Use `ApiResponse<T>.SuccessResponse()`, `SuccessPagedResponse()`, or `FailureResponse()` — never construct manually
- Authorization: `.RequireAuthorization()` on every endpoint, resource-level ownership check inside handler
- One DB per service — never cross DB boundaries
- Async/await throughout, no blocking calls

Before writing any code, briefly state:
1. Which service owns this feature
2. What entity/aggregate is involved
3. What the API contract looks like (method, route, request shape, response shape)

Then implement layer by layer.
