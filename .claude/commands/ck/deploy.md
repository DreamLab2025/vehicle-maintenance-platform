Deploy the Verendar backend.

## Prerequisites check
Before deploying, confirm:
- [ ] `task build` passes locally
- [ ] `task test:all` passes locally
- [ ] All migrations are applied for affected services (`task migrate:add` if pending schema changes)
- [ ] User Secrets / environment variables are set for target environment
- [ ] Docker infrastructure is available (`task docker:dev:up` for local dev)

## Deploy steps

### Local / Dev (via Aspire)
```bash
task docker:dev:up   # Start PostgreSQL, Redis, RabbitMQ (if not using Aspire-managed containers)
task run             # Start all services via Aspire AppHost — gateway on http://localhost:8080
```
Migrations run automatically on startup via `MigrateDbContextAsync<TDbContext>()`.

### Verify deployment
1. Gateway health: `GET http://localhost:8080/health` → 200
2. Service health: `GET http://localhost:8080/alive` → 200
3. Aspire dashboard: `http://localhost:15888` — view logs, traces, service status
4. API docs: each service exposes `/scalar` at its direct port (check Aspire dashboard for port assignments)

## If something goes wrong
- Check Aspire dashboard startup logs — migration errors, missing secrets, and connection failures are logged at startup
- Confirm PostgreSQL and RabbitMQ containers are reachable
- Verify User Secrets are set for each service (`dotnet user-secrets list --project {Service}/Verendar.{Service}`)
- Check `Program.cs` bootstrapping order if startup hangs
