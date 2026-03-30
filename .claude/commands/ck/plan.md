Create detailed implementation plan for: $ARGUMENTS

Flags: `--auto` · `--fast` · `--hard` · `--parallel` · `--no-tasks`

---

## Stage 1 — Research

Use `/ck:scout` to locate relevant files before reading — it's faster than manual glob/grep and surfaces patterns across layers in one pass. Scout the entity, service, repository, and route handler for the affected module, then read what's relevant.

**Always read:**
- `docs/requirements/` — vision, user stories, constraints
- `docs/architecture/` — domain model, ADRs, layer map
- Relevant entity (Domain), service/repository (Application + Infrastructure), and route handler (Api) for the affected module

**`--fast`:** scout + read only direct matches — entity file, service file, route file.

**`--hard`:** also read EF Core migrations, existing tests, cross-service contracts (MassTransit events, typed HTTP clients), and relevant ADRs.

**`--parallel`:** scout and probe Domain + Infrastructure + Api concurrently.

Service ownership:
- Identity: User / Auth / OTP
- Vehicle: Vehicle / Brand / Model / Variant / Type / PartCategory / PartProduct / Odometer
- Garage: Garage / GarageBranch / Mechanic / Booking
- Media: MediaFile · Payment: VNPay · Location: Province / Ward / District
- Ai: Gemini / Questionnaire / Prediction · Notification: Email / SMS / SignalR

Unless `--auto`, confirm key findings before continuing.

---

## Stage 2 — Plan

Use sequential thinking to work through the plan step by step — reason through each layer in order (Domain → Application → Infrastructure → Api), revise if a later layer reveals a conflict with an earlier decision, and branch when a genuine tradeoff requires comparing two paths before committing.

#### Context
- Service/module owner, entities and tables involved
- Current state: what exists vs. what's missing

#### API Contract
- Route, method, request/response shape, status codes, required roles

#### Implementation Steps
Domain → Application → Infrastructure → Api, with specific file names and the change in each.

#### Validation Rules
- Structural: FluentValidation at the request boundary
- Business: ownership checks, permission checks, data integrity inside the service

#### Edge Cases & Risks
- What can go wrong, migration impact, permission/ownership edge cases

#### Alternatives Considered
Brief note on dismissed tradeoffs (if any).

---

## Stage 3 — Tasks

Unless `--no-tasks`, create a TodoWrite task list — one item per layer. Example:

- [ ] Domain: add `X` to `Entity`
- [ ] Infrastructure: migration `AddX`
- [ ] Infrastructure: implement `GetXAsync` in repository
- [ ] Application: update service + DTOs
- [ ] Api: add `POST /x` route

---

Do not write implementation code — only the plan. Flag any assumption that needs confirmation.
