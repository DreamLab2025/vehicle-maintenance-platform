Scan and analyze the Verendar codebase: $ARGUMENTS

Use this to get a broad view of the codebase — architecture health, patterns, potential issues — rather than reviewing a specific piece of code. For reviewing a specific file or feature, use `/review` instead.

## What to scan

Read the argument to understand the focus area. Common uses:

- `analyze architecture patterns` — check layer boundaries, dependency direction, Clean Architecture compliance
- `find N+1 queries` — scan for EF Core queries missing `.Include()` in list endpoints
- `check authorization coverage` — find endpoints missing `.RequireAuthorization()`
- `audit validation coverage` — find request DTOs without a corresponding validator
- `find raw strings` — check for hardcoded messages that should use `AppMessages` or `ValidatorMessages`
- `check soft delete compliance` — find any hard deletes (`.Remove()`) that should be soft deletes

## How to scan

1. Read the relevant layer(s) based on the focus area
2. Look for systematic violations — one-offs are noise, patterns are findings
3. Note file paths and line numbers for each finding

## Output format

Group findings by severity:

**Critical** — correctness or security issues (missing auth, wrong permission check, data leaking across tenants)

**Warning** — architecture violations or pattern inconsistencies (logic in wrong layer, missing validation, raw strings)

**Info** — things to keep an eye on but not urgent

For each finding: file path, line/method, brief description of the issue and why it matters.

End with a short summary: overall health assessment in 2–3 sentences.
