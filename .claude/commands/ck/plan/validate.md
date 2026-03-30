Run `/ck:plan` first (or accept a plan pasted in $ARGUMENTS), then validate each implementation step against the actual codebase.

Topic / plan to validate: $ARGUMENTS

---

## What to validate

For each file named in the plan:

1. **Existence** — does the file exist at the expected path?
2. **Pattern conformance** — do the patterns the plan relies on already exist there? (e.g. `IRepository<T>`, `ToResponse()`, `MapGroup`, `AbstractValidator<T>`)
3. **Schema conflicts** — does the proposed column/field conflict with the EF Core snapshot or existing migrations?
4. **Interface contracts** — if the plan adds a method to an interface, does the interface exist and will the new signature clash?
5. **Convention compliance** — does the proposed code follow project rules (soft delete, `ApiResponse<T>`, no AutoMapper, paginated lists)?

## Output — Validation Report

| Step | Status | Finding |
|---|---|---|
| Add `X` to `Entity` | ✅ Safe | Matches existing nullable pattern |
| Add `GetXAsync` to repo | ⚠️ Risk | Interface already has `GetXAsync` with different signature |
| New migration | ❌ Conflict | Non-nullable column on table with existing rows |
| ... | | |

Status key:
- ✅ Safe — no issues found
- ⚠️ Risk — will work but needs attention
- ❌ Conflict — will break existing code or violate a constraint

## After the report

List all ⚠️ and ❌ items with a suggested fix for each. If all steps are ✅, confirm the plan is ready to implement.

Do not implement anything — only validate and report.
