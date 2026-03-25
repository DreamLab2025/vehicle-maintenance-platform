# Review checklist (cold-read pass)

Use when a dedicated subagent is unavailable. Scan changed files in this order.

## Correctness

- Does the logic match the stated intent and edge cases (null, empty, concurrency)?
- Are error paths handled without leaking secrets or stack traces to clients?

## Security & auth

- Are protected routes and resource ownership checks still correct after the change?
- Any new input surface validated at the boundary (e.g. FluentValidation for DTOs)?

## Contracts

- Public API unchanged unless explicitly in scope (routes, DTO shapes, status codes).
- Cross-service: HTTP client base URLs, message contracts, idempotency where relevant.

## Data & persistence

- Soft delete, migrations, and query filters consistent with the rest of the service?
- N+1 or unbounded queries introduced?

## Tests & verification

- New behavior covered or existing tests updated?
- What command proves build/test for this slice?

## Verendar alignment

- See `backend-development` skill: Minimal API, `ApiResponse<T>`, no AutoMapper, layer placement.
