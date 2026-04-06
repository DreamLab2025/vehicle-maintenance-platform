Fix: $ARGUMENTS

Understand the system before hypothesizing. Read the relevant call chain first — most bugs are obvious once you've seen the whole picture.

If 2+ hypotheses fail, activate `/problem-solving`.

## Gotchas in this codebase

Things that don't look wrong until you check:

- **Soft delete filter**: query returns empty/partial results → `DeletedAt == null` filter missing. EF global query filters only apply to entities explicitly configured for them in `BaseDbContext`.
- **EF tracking**: `InvalidOperationException` on update → entity loaded with `AsNoTracking()` then modified, or loaded in one scope and mutated in another.
- **Auth 403**: check both `RequireAuthorization()` on the endpoint AND that the JWT claims match the expected policy/role.
- **Validator bypassed**: validation not running → validator not registered in DI, or `ValidateAsync()` not called at the service call site.
- **Lost mutations**: entity updated but not persisted → `IUnitOfWork.SaveChangesAsync()` not called.
- **Silent N+1**: navigation property accessed without a matching `.Include()` in the query.
- **Inter-service call**: typed HTTP client returns error → check Aspire service discovery name matches `Configuration["Services:{Name}:BaseUrl"]` override.
