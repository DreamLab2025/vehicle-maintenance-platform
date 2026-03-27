Summarize the Verendar codebase and create an overview document.

## What to cover

Read the codebase and produce a clear summary covering:

### 1. Project purpose
- What problem does this system solve?
- Who are the users? (derive from role constants and API surface)

### 2. Domain model
- List all aggregates/entities per service (`{Service}/Verendar.{Service}.Domain/Entities/`)
- Group by service: Identity, Vehicle, Garage, Media, Notification, Payment, Location, Ai
- Note key relationships and cross-service event contracts

### 3. API surface
- List all route groups from each service's Host `Apis/` directory
- For each group: what it manages, gateway route prefix

### 4. Key architectural decisions
- Microservices via Aspire, each service: Domain / Application / Infrastructure / Host
- Patterns enforced (Minimal API, FluentValidation, soft delete, `ApiResponse<T>`, MassTransit)
- External dependencies (PostgreSQL per service, RabbitMQ, Redis, VNPay)

### 5. What's missing / in progress
- TODOs in code
- Entities with no endpoints yet
- Features partially implemented

## Output format

Write the summary to `docs/OVERVIEW.md` (create if absent, overwrite if present).

Keep it under 200 lines — scannable, not exhaustive. Use bullet lists and short sentences. A new team member should be able to read this in 5 minutes and know where to start.
