Update existing documentation to reflect recent code changes.

## What triggers this
- New entities or migrations added
- API routes added, removed, or renamed
- Business rules or constants changed
- Architecture decisions made (new pattern, removed pattern)
- New domain modules added (e.g. new aggregate)

## Step 1 — Identify what changed
```bash
git log --oneline -20
git diff main...HEAD --name-only
```

Read the diff to understand scope of changes.

## Step 2 — Update the relevant docs

| Changed area | Update this doc |
|---|---|
| New/modified entity | `docs/design/*.md` — table schema |
| New API endpoint | `docs/architecture/integrations.md` or relevant module doc |
| New domain constant | `docs/architecture/domain-model.md` |
| New architectural pattern | `docs/architecture/layers.md` or new ADR |
| New business rule | `docs/requirements/user-stories.md` or `constraints.md` |

## Step 3 — Write clearly

- Document intent, not just structure — why a decision was made, not just what it is
- If a decision was made under constraints, note the constraint
- Keep it short: a doc no one reads is worse than no doc

## What not to change
- Do not rewrite docs that are still accurate
- Do not delete ADRs — mark deprecated ones with `**Status**: Deprecated` and explain why
- Do not add docs for things already obvious from reading the code
