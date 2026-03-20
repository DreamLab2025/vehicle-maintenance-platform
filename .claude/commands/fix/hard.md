Investigate and fix the following complex issue in the Verendar backend: $ARGUMENTS

This requires deep investigation before any code changes. Do not jump to fixing until you understand the root cause.

## Step 1 — Understand the system
- Read the relevant domain entities, application services, and infrastructure layers
- Trace the full call path from API request to database and back
- Identify all the places the broken behavior could originate
- Check `docs/architecture/` for expected domain model and ADRs if data-related

## Step 2 — Identify root cause
- State the exact root cause (specific file, line, incorrect assumption)
- Explain why this isn't obvious (hidden coupling, async timing, EF tracking issue, permission logic, etc.)

## Step 3 — Plan the fix
- List every file that needs to change and why
- Check for side effects: migration needed? contract change? permission mapping change?

## Step 4 — Implement
- Fix layer by layer following Clean Architecture (Domain → Application → Infrastructure → Api)
- Write or update tests if applicable
- Confirm no regressions in adjacent behavior
