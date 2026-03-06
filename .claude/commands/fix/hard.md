Investigate and fix the following complex issue: $ARGUMENTS

This requires deep investigation before any code changes. Do not jump to fixing until you understand the root cause.

## Step 1 — Understand the system
- Read the relevant domain entities, application services, and infrastructure layers
- Trace the full call path from request to database and back
- Identify all the places the broken behavior could originate

## Step 2 — Identify root cause
- State the exact root cause (specific file, line, incorrect assumption)
- Explain why this isn't obvious (hidden coupling, async timing, EF tracking issue, etc.)

## Step 3 — Plan the fix
- List every file that needs to change and why
- Check for side effects: migration needed? contract change? event behavior change?

## Step 4 — Implement
- Fix layer by layer following Clean Architecture
- Write or update tests if applicable
- Confirm no regressions in adjacent behavior
