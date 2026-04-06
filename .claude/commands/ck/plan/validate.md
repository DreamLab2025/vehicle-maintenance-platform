Run a validation pass against an existing plan or feature description: $ARGUMENTS

This subcommand runs Step 5 (Validate) in isolation.
Use it after a plan exists and you want a focused feasibility check without redoing the full pipeline.

`$ARGUMENTS` can be:

- A feature name → look for `docs/plans/<slug>.md` first, fall back to current conversation context.
- A file path → read that file directly.
- Empty → validate the most recent plan in the current conversation.

---

## Validation Checks

Run each check and report **Pass**, **Fail**, or **Warning** with a one-line reason.

### Migration Safety

- [ ] Does this plan add a NOT NULL column? If yes: does it have a default value or is it applied after a backfill?
- [ ] Is the migration reversible without data loss?
- [ ] Does the migration touch a high-volume table? Flag for index review.

### Auth / Ownership

- [ ] Is the endpoint properly protected with `RequireAuthorization()`?
- [ ] Is resource ownership checked inside the service (not just at the route level)?
- [ ] Are anonymous callers explicitly blocked where required?

### Query Safety

- [ ] Does any new query produce an N+1 (missing `.Include()` or separate loop query)?
- [ ] Are paginated queries using `GetPagedAsync()` with `PaginationRequest`?
- [ ] Are any unbounded queries introduced outside the Location service?

### Response Contract

- [ ] Does the success response match `ApiResponse<T>` with the correct `isSuccess: true` shape?
- [ ] Are all failure paths returning the correct failure response — no raw strings?
- [ ] Do handlers call `.ToHttpResult()`, not `Results.Ok()` directly?

### Layer Integrity

- [ ] Does the plan keep `{Service}.Application/GlobalUsings.cs` free of Infrastructure namespaces?
- [ ] Are new DTOs placed in `Application/` (not `Domain/` or host project)?
- [ ] Does the Domain layer remain free of external package dependencies?

### Cross-service

- [ ] If this raises a MassTransit event: does the contract extend `BaseEvent`? Is it in a `Contracts` project?
- [ ] If this calls another service via typed HTTP client: is the client registered in `Bootstrapping/`?

---

## Output Format

```
## Validation Report: <Feature Name>

| Check | Result | Note |
|---|---|---|
| NOT NULL default | Pass | Column is nullable |
| Unbounded query | Warning | Location service — intentional, uses 24h Redis cache |
| N+1 query | Fail | GetMembers loop missing .Include() |
...

### Action Items
- [Fail] Fix N+1: add `.Include()` to GetMembers query in Step 4.
- [Warning] Confirm: is this Location service query intentionally unbounded?
```

---

Do not rewrite the plan — only report findings and required action items.
Fail items block implementation. Warnings need confirmation before proceeding.
