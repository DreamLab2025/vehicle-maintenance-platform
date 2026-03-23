Investigate and fix the following complex issue in the Verendar backend: $ARGUMENTS

This requires deep investigation before any code changes. Do not jump to fixing until you understand the root cause.

## Step 1 — Understand the system
- Read the relevant domain entities, application services, and infrastructure layers
- Trace the full call path from API request to database and back
- Identify all the places the broken behavior could originate
- Check cross-service interactions: MassTransit consumers, IPaymentClient HTTP calls, SignalR hubs

## Step 2 — Identify root cause
- State the exact root cause (specific file, line, incorrect assumption)
- Explain why this isn't obvious (hidden coupling, async timing, EF tracking issue, event ordering, soft-delete filter, etc.)

## Step 3 — Plan the fix
- List every file that needs to change and why
- Check for side effects: migration needed? contract change? consumer registration change?

## Step 4 — Implement
- Fix layer by layer (Domain → Application → Infrastructure → Host)
- Write or update tests if applicable (see `Location/Verendar.Location.Tests` for patterns)
- Confirm no regressions in adjacent behavior
