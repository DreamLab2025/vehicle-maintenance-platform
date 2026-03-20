Research and create an implementation plan for: $ARGUMENTS

Explore relevant parts of the codebase first — read existing patterns, similar endpoints, domain entities before proposing anything.

Reference `docs/` for design decisions:
- `docs/requirements/` — vision, scope, constraints
- `docs/architecture/` — domain model, layers, integrations, ADRs

## Structure

### Context
- Which service and aggregate owns this
- Existing entities/tables involved
- Current state (what exists vs missing)

### API Contract
- Route, method, request/response shape
- HTTP status codes
- Required JWT role (User, GarageOwner, Mechanic, Admin)

### Implementation Steps
Layer-by-layer (Domain → Application → Infrastructure → Host), specific file names and changes.

### Validation Rules
- Structural (FluentValidation at boundary)
- Business (inside service — ownership, data integrity)

### Edge Cases & Risks
- Migration impact (new table/column or logic only)
- Cross-service event impact (MassTransit)

Do not write implementation code — only the plan. Flag assumptions that need confirmation.
