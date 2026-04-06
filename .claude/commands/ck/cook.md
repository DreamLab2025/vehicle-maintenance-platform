Implement the following feature for the Verendar .NET backend: $ARGUMENTS

## Flags

| Flag            | Effect |
| --------------- | ------ |
| `--auto`        | No pauses. Default. |
| `--interactive` | Pause for confirmation between phases. |
| `--fast`        | Skip plan lookup, implement from Scout findings. |
| `--parallel`    | Scout + plan lookup as concurrent subagents. |

## Before Writing Code

Check `docs/plans/` for a plan matching this feature (glob by name or slug).
- **Found** → use it as the blueprint, skip all planning.
- **Not found** → skip planning, implement from Scout findings.

Understand the existing pattern before writing anything new — the closest existing feature in the same service is the best guide for naming, response shapes, and query style.

## Gotchas

These compile fine but are wrong in this codebase:

- **`GlobalUsings.cs`**: each project has its own. `{Service}.Application/GlobalUsings.cs` must not reference Infrastructure types — violates the layer boundary silently.
- **Responses**: services return `ApiResponse<T>`, handlers return `.ToHttpResult()`. Using `Results.Ok()` directly bypasses the response contract.
- **Saving**: call `IUnitOfWork.SaveChangesAsync()` after mutations. Easy to omit in services that create/update entities.
- **Soft delete queries**: filter `DeletedAt == null` on any entity that participates in soft delete, unless intentionally overriding the global filter.
- **Pagination**: all lists go through `PaginationRequest` + `GetPagedAsync()`. Exception: Location service uses unbounded lists with 24h Redis cache.
- **MassTransit events**: all events extend `BaseEvent`. Consumers are auto-discovered — no manual registration needed.

## Finalize

Run `/code-review` on all changed files.
