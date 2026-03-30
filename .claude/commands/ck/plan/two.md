Research and create an implementation plan with two alternative approaches for: $ARGUMENTS

Follow the same exploration steps as `/plan`, then present exactly two approaches.

## Structure each approach as

### Approach A — [Short Name]

**Summary**: One sentence description of the approach.

**Implementation steps**: Layer-by-layer breakdown (Domain → Application → Infrastructure → Api)

**Tradeoffs**:

- Pros: what this makes easier
- Cons: what this complicates or costs
- Best when: the condition under which this approach is preferable

---

### Approach B — [Short Name]

(same structure)

---

## Recommendation

State which approach you recommend and why, given the current codebase state and project constraints (RBAC model, soft delete, EF Core patterns in use).

If the choice depends on something only the team knows (e.g. expected query volume, future extensibility plans), name that dependency explicitly instead of guessing.

Do not write implementation code — only the plan.
