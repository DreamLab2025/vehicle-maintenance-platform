Check deployment readiness for the Verendar backend.

Run each check and report pass/fail with a short explanation for any failure.

## Checklist

### Build
```bash
task build
```
Must complete with 0 errors, 0 warnings that would break runtime.

### Tests
```bash
task test:all
```
All tests must pass.

### Pending migrations
Check if there are pending EF Core migrations for each modified service:
```bash
dotnet ef migrations list \
  --project {Service}/Verendar.{Service}.Infrastructure \
  --startup-project {Service}/Verendar.{Service}
```
Flag any migration that shows `(Pending)`. Run with `task migrate:add NAME=... PROJECT=... STARTUP=...` if needed.

### User Secrets / environment variables
Verify these are set for each service in the target environment:
- `ConnectionStrings__{ServiceDatabase}` — PostgreSQL connection string per service
- `RabbitMQ__ConnectionString` — RabbitMQ connection string
- `Redis__ConnectionString` — Redis (if service uses it)
- `Jwt__SecretKey` — JWT signing key (Identity service, must be ≥ 32 chars)

### Git status
```bash
git status
git log origin/main..HEAD --oneline
```
No uncommitted changes. All commits pushed to remote.

### Docker infrastructure
```bash
docker ps
```
PostgreSQL, RabbitMQ, Redis containers must be running (or Aspire is managing them via `task run`).

## Output format

Report each section as `✓ PASS` or `✗ FAIL: <reason>`. Summarize at the end: ready to deploy or blocked (list blockers).
