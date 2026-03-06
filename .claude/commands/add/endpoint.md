Add a new Minimal API endpoint to the Verendar backend: $ARGUMENTS

Read the existing endpoints in the target service's `Apis/` folder before writing anything new.

## Required steps

1. **Request DTO** — create in `Application/Dtos/`, with FluentValidation validator in `Application/Validators/`. Vietnamese error messages for user-facing fields.

2. **Response DTO** — create in `Application/Dtos/`. Add `ToResponse()` mapping extension in `Application/Mappings/`.

3. **Service/handler method** — add to the relevant service in `Application/Services/`. Include resource ownership check if the endpoint accesses user-owned data.

4. **Repository method** — if new query needed, add interface in `Domain/Repositories/` and implementation in `Infrastructure/Repositories/`.

5. **Endpoint** — add to the appropriate `Apis/` file:
   ```csharp
   group.MapGet("/path", HandlerMethod).RequireAuthorization();
   ```
   Register in the `Map*Endpoints()` method in `Bootstrapping/`.

6. **Response wrapping** — always use `ApiResponse<T>`:
   - `Results.Ok(ApiResponse<T>.SuccessResponse(dto))` — single item (200)
   - `Results.Ok(ApiResponse<T>.SuccessPagedResponse(...))` — list (200)
   - `Results.Created(location, ApiResponse<T>.SuccessResponse(dto))` — create (201)
   - `Results.Ok(ApiResponse<T>.FailureResponse("message"))` — business failure (200 with isSuccess:false)
   - `Results.NotFound(ApiResponse<T>.FailureResponse("..."))` — 404
   - `Results.Forbid()` — 403 ownership violation

Do not use Controllers. Do not add AutoMapper.
