Run `/ck:plan` first (or accept a plan pasted in $ARGUMENTS), then save it to docs.

Topic / plan to archive: $ARGUMENTS

---

Save the finalized plan to:

```
docs/plans/<kebab-case-topic>-plan.md
```

If the `docs/plans/` directory does not exist, create it first.

## File format

```markdown
# Plan: <Topic>
_Created: <YYYY-MM-DD>_

## Context
...

## API Contract
...

## Implementation Steps
...

## Validation Rules
...

## Edge Cases & Risks
...

## Alternatives Considered
...
```

Include every section from the plan. If the plan used `--two`, include both approaches and the recommendation.

Confirm the save path before writing, unless the user said `--auto`.
