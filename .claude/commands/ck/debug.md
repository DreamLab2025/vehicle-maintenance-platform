Investigate and fix the following issue in the Verendar backend: $ARGUMENTS

Read the relevant code before hypothesizing — most bugs are obvious after seeing the whole picture.

## Diagnosis

- **Root cause**: specific file/line/query and why it's wrong
- **Why undetected**: missing test? wrong assumption? hidden coupling?
- **Fix**: minimal, scoped to root cause

## Common failure patterns

- **Empty or wrong data**: missing `DeletedAt == null` filter — soft-deleted records appear in queries when the global EF filter isn't configured for that entity
- **EF tracking error**: entity loaded with `AsNoTracking()` then modified, or loaded in one scope and mutated in another
- **Silent N+1**: navigation property accessed in a loop without `.Include()` in the query
- **403 despite correct role**: `RequireAuthorization()` configured incorrectly on the endpoint, or JWT claims not matching expected policy
- **Validation not running**: validator not registered in DI, or `ValidateAsync()` missing at the call site
- **Mutation not persisted**: `IUnitOfWork.SaveChangesAsync()` not called after entity change
- **Inter-service call failure**: typed HTTP client base URL not configured in Aspire service discovery; check `Configuration["Services:{Name}:BaseUrl"]`
