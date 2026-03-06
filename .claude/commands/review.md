Review the following code for quality and correctness: $ARGUMENTS

Read all referenced files before commenting. Understand intent before criticizing.

## Review checklist

**Correctness**
- Does the logic do what it intends?
- Are edge cases (null, empty list, concurrent access) handled?
- Is resource ownership verified for user-scoped data?

**Architecture**
- Is business logic in Application, not Host or Infrastructure?
- Does Infrastructure depend only on Domain interfaces?
- Are cross-service calls done via HTTP or events (not shared DB)?

**API contract**
- Is the response wrapped in `ApiResponse<T>`?
- Are paginated endpoints using `PaginationRequest` + `SuccessPagedResponse`?
- Are HTTP status codes correct (201 for create, 204 for void delete, 404 for not found)?

**Code quality**
- Are mapping methods static extensions (not AutoMapper)?
- Is FluentValidation used for request DTOs?
- Are async methods actually awaited? No `.Result` or `.Wait()`?
- Are nullable reference types handled correctly?

**Security**
- Is `.RequireAuthorization()` on every non-public endpoint?
- Are no secrets hardcoded or logged?

## Output format
Group findings by: **Must fix** / **Should fix** / **Consider**. Skip categories with no findings.
