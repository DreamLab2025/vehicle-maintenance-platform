Review the following code for quality and correctness: $ARGUMENTS

Read all referenced files before commenting. Understand intent before criticizing.

## Checklist

**Correctness**
- Does the logic do what it intends?
- Are edge cases (null, empty list, concurrent access) handled?
- Is resource ownership verified for user-scoped data?

**Architecture**
- Is business logic in Application/Services, not Host or Infrastructure?
- Does Infrastructure depend only on Domain interfaces?
- Are domain entities free of framework dependencies?

**Authorization**
- Is JWT role check correct? (User / GarageOwner / Mechanic / Admin)
- Is resource ownership verified after role check?

**API contract**
- Are paginated endpoints using `PaginationRequest` + `GetPagedAsync`?
- Are HTTP status codes correct? (201 create, 204 delete, 404 not found)
- Is `.RequireAuthorization()` on every non-public endpoint?
- Is response wrapped in `ApiResponse<T>`?

**Code quality**
- Are mapping methods static extensions (`ToResponse()`, `ToEntity()`), not AutoMapper?
- Is FluentValidation used for structural rules; business rules inside service?
- Are async methods actually awaited? No `.Result` or `.Wait()`?
- Soft delete used (`DeletedAt = DateTime.UtcNow`), not `DbContext.Remove()`?

**Security**
- Are no secrets hardcoded or logged?
- Is file upload size validated?

## Output
Group findings: **Must fix** / **Should fix** / **Consider**. Skip empty categories.
