Create initial documentation structure for the Verendar backend.

## Scaffold `docs/` layout

Create the following structure if it does not exist:

```
docs/
├── requirements/
│   ├── vision.md          — project goals, stakeholders, success criteria
│   ├── scope.md           — in-scope features, out-of-scope items
│   ├── user-stories.md    — user roles and their key workflows
│   └── constraints.md     — tech constraints, compliance, performance requirements
├── architecture/
│   ├── domain-model.md    — aggregates, entities, value objects per service
│   ├── layers.md          — CA layer responsibilities and dependency rules
│   ├── services.md        — inter-service communication (MassTransit events + typed HTTP clients)
│   └── decisions/         — ADRs (Architecture Decision Records)
│       └── adr-001-template.md
├── design/
│   ├── 01-identity-tables.md
│   ├── 02-vehicle-tables.md
│   ├── 03-garage-tables.md
│   └── 04-payment-tables.md
└── plans/                 — implementation plans (one file per feature)
```

## What to populate

1. **Read** each `{Service}/Verendar.{Service}.Domain/Entities/` to derive the domain model per service
2. **Read** each `{Service}/Verendar.{Service}/Apis/` to list all exposed API surfaces
3. **Read** `CLAUDE.md` for layer rules and key architectural decisions
4. Populate each file with what is derivable from the code — do not invent requirements
5. Mark any section `<!-- TODO: confirm with team -->` where intent is unclear from code alone

## ADR template (`adr-001-template.md`)

```markdown
# ADR-001: [Decision Title]

**Status**: Proposed / Accepted / Deprecated
**Date**: YYYY-MM-DD

## Context
What problem are we solving?

## Decision
What did we decide?

## Consequences
What becomes easier or harder as a result?
```

Do not create files that are already present. Update them instead.
