Run `/ck:plan` first (or accept a plan pasted in $ARGUMENTS), then challenge it hard.

Topic / plan to red-team: $ARGUMENTS

---

## Red-team checklist

Work through each angle systematically:

1. **Wrong assumptions** — what does the plan take for granted that might not be true?
   - Client always sends valid data?
   - Entity always exists when referenced?
   - External service always responds?

2. **Uncovered edge cases**
   - Soft-deleted records referenced by new queries
   - Concurrent modifications (two users acting on the same resource simultaneously)
   - Empty collections, null foreign keys, boundary values

3. **Permission model gaps**
   - Can a non-owner trigger this endpoint?
   - Is the admin vs. resource-owner distinction handled?
   - Does the plan check ownership at the right layer (service, not just route)?

4. **Volume and performance risks**
   - N+1 queries introduced by the new code path
   - Missing indexes for new filter/sort columns
   - Unbounded list without pagination

5. **Migration hazards**
   - Non-nullable column added to a table with existing rows (needs default or migration strategy)
   - Column renamed or dropped that is still referenced by existing code or other migrations

6. **Cross-service fragility**
   - MassTransit consumer failure path — what happens if the consumer throws?
   - Typed HTTP client — timeout, circuit breaker, retry policy considered?
   - Event contract change — will existing consumers break?

7. **Validation gaps**
   - Business rules that FluentValidation cannot catch (ownership, uniqueness, state transitions)
   - Missing service-layer guards for those rules

---

## Output — Red-Team Report

For each finding:

| #   | Area        | Finding                                | Severity | Mitigation                                          |
| --- | ----------- | -------------------------------------- | -------- | --------------------------------------------------- |
| 1   | Permissions | Non-owner can call DELETE endpoint     | High     | Add ownership check in service before delete        |
| 2   | Performance | `GetAllBranchesAsync` lacks pagination | Medium   | Wrap with `PaginationRequest` + `GetPagedAsync`     |
| 3   | Migration   | New `Rating` column is non-nullable    | High     | Set `defaultValue: 0` in migration or make nullable |
| ... |             |                                        |          |                                                     |

Severity:

- **High** — will cause bugs in production or break existing behavior
- **Medium** — risk under certain conditions or at scale
- **Low** — minor, acceptable to defer

## After the report

Address all **High** findings by revising the plan. Present the revised plan after the report. Medium and Low findings are called out but left to the team to decide.

Do not implement anything — only red-team and report.
