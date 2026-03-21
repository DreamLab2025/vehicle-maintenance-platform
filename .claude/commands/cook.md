Implement the following feature for the Verendar .NET backend: $ARGUMENTS

Scope: full feature (entity → repo → service → DTOs → API) or single function. Only touch layers that change.

## Before coding
1. Which service owns this (Identity, Vehicle, Media, Notification, Ai)?
2. Which aggregate and entities are involved?
3. Request/response shape and status codes

## Follow
- Architecture checklist: `.claude/skills/backend-development/references/architecture.md`
- API conventions: `.claude/skills/backend-development/references/api-design.md`
- CLAUDE.md: no AutoMapper, Minimal API only, soft delete, paginate lists, no MediatR

Adding one route to an existing module → use **Add endpoint** command instead.
