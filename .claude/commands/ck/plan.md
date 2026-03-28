Research and create an implementation plan for: $ARGUMENTS

Explore the relevant parts of the codebase first — read existing patterns, similar endpoints, domain entities, and infrastructure before proposing anything.

Reference the project documentation in `docs/` for design decisions:
- `docs/requirements/` — vision, scope, user stories, constraints
- `docs/architecture/` — domain model, layers, integrations, ADRs

Structure your plan as:

## Context
- Which service/module owns this (Identity: User/Auth; Vehicle: Vehicle/Brand/Model/Variant/Type/PartCategory/PartProduct; Garage: Garage/Branch/Mechanic/Booking; Media: MediaFile; Payment; Location; Ai)
- Which existing entities/tables are involved
- Current state (what exists vs what's missing)

## API Contract
- Route, method, request shape, response shape
- HTTP status codes for success and each failure case
- Required permissions (which roles can access)

## Implementation Steps
Layer-by-layer breakdown (Domain → Application → Infrastructure → Api), with specific file names and what changes in each.

## Validation Rules
- Structural (FluentValidation at boundary)
- Business (inside service — ownership check, permission check, data integrity)

## Edge Cases & Risks
- What can go wrong
- Migration impact (new table, column, or just logic)
- Permission/ownership edge cases

## Alternatives Considered
If there are meaningful tradeoffs, list them briefly.

Do not write implementation code — only the plan. Flag any assumptions that need confirmation.
