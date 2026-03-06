Research and create an implementation plan for: $ARGUMENTS

Explore the relevant parts of the codebase first — read existing patterns, similar endpoints, domain entities, and infrastructure before proposing anything.

Structure your plan as:

## Context
- Which service owns this
- Which existing entities/tables are involved
- Current state (what exists vs what's missing)

## API Contract
- Route, method, request shape, response shape
- HTTP status codes for success and each failure case

## Implementation Steps
Layer-by-layer breakdown (Domain → Application → Infrastructure → Host), with specific file names and what changes in each.

## Validation Rules
- Structural (FluentValidation at boundary)
- Business (inside service/handler)

## Edge Cases & Risks
- What can go wrong
- Migration impact (new table, column, or just logic)
- Cross-service effects (event vs HTTP call)

## Alternatives Considered
If there are meaningful tradeoffs, list them briefly.

Do not write implementation code — only the plan. Flag any assumptions that need confirmation.
