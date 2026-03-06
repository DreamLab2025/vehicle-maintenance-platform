# Backend DevOps (2025)

## DevOps Is About Reducing Time-to-Confidence

Every DevOps practice — containerization, migration automation, secrets management, background jobs — exists to reduce the gap between "I wrote code" and "I'm confident it works in production." Think about each tool through that lens.

---

## Orchestration — Let the Platform Handle Plumbing

.NET Aspire is the orchestration layer. It handles service discovery, connection strings, and infrastructure provisioning so that application code never hardcodes these concerns.

**The key principle**: services don't know their own infrastructure addresses. They ask the orchestrator. This means the same codebase runs locally with Docker, in CI with mocked resources, and in production with real cloud services — without code changes.

```csharp
// AppHost wires everything. Services just declare dependencies.
var vehicleService = builder.AddProject<Projects.Verendar_Vehicle>("vehicle-service")
    .WithReference(vehicleDb)
    .WithReference(rabbitMq);
```

**When Aspire fits**: .NET-heavy stacks where you control the orchestration layer. **When it doesn't**: polyglot systems where other teams own different services — use Kubernetes or Docker Compose instead.

---

## Infrastructure Mindset — Isolate by Service

Each service owns its own database. This isn't just a microservices convention — it's a practical constraint that prevents one team's schema migration from breaking another service.

The cost: you need multiple databases locally. The benefit: you can evolve each schema independently, run migrations per-service, and scale databases differently.

**Decision point**: if two services share a database, they're effectively one service from a deployment perspective. Either merge them or truly isolate them.

Current project infra:
- Postgres 17 (identity-db, vehicle-db, media-db, notification-db, ai-db)
- RabbitMQ 3 (async messaging)
- Redis (caching, session)
- YARP (reverse proxy / API gateway at port 8080)

---

## Database Migrations — Automate, Don't Manually Script

Running migrations on startup (`MigrateDbContextAsync<T>()`) removes an entire class of deployment error: "someone forgot to run migrations before deploying."

The tradeoff: startup is slightly slower, and you must ensure migrations are backward-compatible with the running code (in rolling deploys). For this project's scale, the simplicity wins.

**Good migration hygiene**:
- One migration per logical change — don't batch unrelated schema changes
- Migration names describe intent: `AddPartCategoryTable`, not `Migration_20250301`
- Test migrations by running them against a copy of production data occasionally
- Never edit an existing migration that's already been applied to any environment

```bash
dotnet ef migrations add AddPartCategoryTable \
  --project Verendar.Vehicle.Infrastructure \
  --startup-project Verendar.Vehicle
```

---

## Secrets — The Simplest Security Win

Source control is permanent. A secret committed to git stays in history even after deletion. User Secrets (local development) and environment variables / secret stores (production) keep credentials out of the codebase entirely.

**The mental model**: `appsettings.json` is documentation — what config keys exist and their non-sensitive defaults. The actual values for sensitive keys come from elsewhere at runtime.

```json
// appsettings.json — safe to commit, documents the key exists
{ "Gemini": { "Model": "gemini-2.0-flash" } }

// User Secrets / env var — never committed
{ "Gemini": { "ApiKey": "actual-secret" } }
```

Review every `appsettings.json` change before committing. If a key's value looks like a secret (API key, password, connection string with credentials), it doesn't belong there.

---

## Background Jobs — Decouple Time from Request

Hangfire (or any job scheduler) exists to answer: "what work should happen on a schedule, independent of user requests?"

**Good candidates for background jobs**:
- Daily recalculation of statuses (maintenance reminder levels)
- Sending batched notifications
- Cleaning up stale data
- Expensive sync operations that don't need real-time results

**Bad candidates**:
- Work that the user is waiting for (this should be in the request/response cycle)
- Work that must be consistent with the triggering transaction (use domain events instead)

The current jobs (`OdometerReminderJob`, `MaintenanceReminderJob`) run daily and sync reminder levels for all vehicles — work that doesn't need to happen in real-time but would be expensive to compute on every read.

**Monitoring jobs**: failed jobs need to be visible. Hangfire's dashboard (or equivalent) should be accessible in every environment. Silent job failures are production incidents you don't know about yet.

---

## API Gateway (YARP) — One Entry Point

The gateway pattern gives clients a stable address while the internal routing can change. Adding a new service means updating the gateway config, not telling every client about a new address.

**Route design in the gateway** follows service ownership: every `/api/v1/vehicle-*` path goes to the vehicle service. When you add a new resource to vehicle service, it's covered by the existing route pattern — no gateway change needed.

**What the gateway should not do**: business logic. It's infrastructure — routing, rate limiting, auth token forwarding. The moment the gateway starts making domain decisions, it becomes a coupling point.

---

## Running Locally — Fast Feedback Loop

The fastest way to catch bugs is locally, before CI. Aspire makes the full stack runnable with one command:

```bash
dotnet run --project Verendar.AppHost
```

This starts all services, all infrastructure containers, and the Aspire dashboard. Use the dashboard to watch logs across services simultaneously — invaluable for debugging cross-service flows.

For isolated service development: run infrastructure via Docker Compose, then start only the service you're working on. Faster iteration, same confidence.
