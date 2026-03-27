Implement the following feature or function for the Verendar .NET backend: $ARGUMENTS

Scope: full feature (entity → repo → service → DTOs → API) or single function (one endpoint/method). Only touch layers that change.

## Before coding
1. Owner: which service/aggregate (Identity: User; Vehicle: Vehicle/Brand/Model/Variant/Type/PartCategory/PartProduct; Garage: Garage/Branch/Mechanic/Booking; Media: MediaFile; Payment: Payment; Location: Province/Ward; …)
2. Scope: entities and API surface (method, route, request/response)
3. Contract: request shape, response shape, status codes

## Follow
- Architecture feature checklist: `.claude/skills/backend-development/references/architecture.md`
- API conventions (routes, auth, pagination, status codes): `.claude/skills/backend-development/references/api-design.md`
- Core rules (CLAUDE.md): no AutoMapper, Minimal API only, soft delete, paginate lists, RBAC

Adding one route to an existing module → use **Add endpoint** command instead.
