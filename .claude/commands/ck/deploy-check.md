Check deployment readiness for the Verendar backend.

Run each check and report pass/fail with a short explanation for any failure.

## Checklist

### Build
```bash
task build
```
Must complete with 0 errors.

### Tests
```bash
task test:all
```
All tests must pass.

### Pending migrations
Check if there are pending EF Core migrations per service:
```bash
task migrate:list PROJECT={Service}/Verendar.{Service}.Infrastructure STARTUP={Service}/Verendar.{Service}
```
Flag any migration that shows `(Pending)`.

### Environment variables (`.env.prod`)
Verify these are set:
- `GATEWAY_PORT` — should be `80` for Cloudflare proxy
- `JWT_BEARER_ISSUER` / `JWT_BEARER_AUDIENCE` — must be `https://api.verendar.vn`
- `CORS_ALLOWED_ORIGINS` — frontend domains (e.g. `https://verendar.vn`)
- Per-service DB names: `IDENTITY_SERVICE_DATABASE`, `VEHICLE_SERVICE_DATABASE`, `MEDIA_SERVICE_DATABASE`, `NOTIFICATION_SERVICE_DATABASE`, `AI_SERVICE_DATABASE`, `LOCATION_SERVICE_DATABASE`, `GARAGE_SERVICE_DATABASE`
- DB/Redis/RabbitMQ passwords, JWT secret, Resend key, S3 credentials

### Git status
```bash
git status
git log origin/main..HEAD --oneline
```
No uncommitted changes. All commits pushed to remote.

### Docker containers (prod check via SSH)
```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps
curl -s http://localhost/health
```
All service containers must be running; health endpoint returns `healthy`.

## Output format

Report each section as `✓ PASS` or `✗ FAIL: <reason>`. Summarize at the end: ready to deploy or blocked (list blockers).
