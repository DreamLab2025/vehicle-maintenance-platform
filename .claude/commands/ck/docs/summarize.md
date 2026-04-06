Summarize the Verendar codebase and create an overview document.

## What to cover

Read the codebase and produce a clear summary covering:

### 1. Project purpose
- What problem does this system solve? (vehicle maintenance platform)
- Who are the users? (derive from API surfaces and auth patterns)

### 2. Domain model per service
- List all aggregates/entities in each `{Service}/Verendar.{Service}.Domain/Entities/`
- Group by service: Identity · Vehicle · Media · Notification · Ai · Garage · Payment · Location
- Note relationships and cross-service event flows

### 3. API surface
- List all route groups from each `{Service}/Verendar.{Service}/Apis/`
- For each group: what it manages, auth requirements

### 4. Key architectural decisions
- Microservices with Aspire orchestration (local) / Docker Compose (prod)
- Clean Architecture layers per service
- Patterns enforced: Minimal API, FluentValidation, soft delete, `ApiResponse<T>`, `PaginationRequest`
- Inter-service: MassTransit/RabbitMQ (async) + typed HTTP clients (sync)
- External: PostgreSQL (per-service DB), Redis, RabbitMQ, Cloudflare/EC2 (prod)

### 5. What's missing / in progress
- TODOs in code
- Entities with no endpoints yet
- Features partially implemented

## Output format

Write the summary to `docs/OVERVIEW.md` (create if absent, overwrite if present).

Keep it under 200 lines — scannable, not exhaustive. Use bullet lists and short sentences. A new team member should be able to read this in 5 minutes and know where to start.
