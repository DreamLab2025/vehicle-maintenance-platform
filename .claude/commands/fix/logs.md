Analyze application logs and fix the identified issue: $ARGUMENTS

## Where to find logs

**Local dev (console):**
- Terminal running `task run` — structured logs via OpenTelemetry
- Aspire dashboard at `http://localhost:15888` — searchable log/trace UI per service

**Production:**
- Check deployment-specific log sink (configured via Aspire / environment variables)

## Step 1 — Read and classify the log entries

Look for:
- `Error` / `Fatal` level entries with stack traces
- Repeated warnings that indicate degradation (connection timeouts, retry loops)
- Slow queries logged by EF Core (look for `Executed DbCommand` with high duration)
- 4xx/5xx HTTP responses in the request pipeline log
- Memory pressure (heap size climbing, GC warnings)

## Step 2 — Trace to source

Map the log entry back to code:
- Stack trace → specific file and line
- `RequestId` correlation → trace the full request lifecycle
- `UserId` or entity ID in log context → which resource was being accessed

## Step 3 — Fix

- **NullReferenceException / KeyNotFoundException** — defensive check missing or wrong assumption about data state
- **EF Core slow query** — likely an N+1; add `.Include()` or rewrite as projection
- **Repeated 401/403** — permission or token config issue; check `role_permissions` seed data
- **Connection pool exhaustion** — `DbContext` not being disposed, or too many concurrent requests
- **Memory leak** — IDisposable not disposed, static collection growing unbounded

Apply the minimal fix. Do not refactor surrounding code unless it is the root cause.

## Step 4 — Verify
```bash
task run
```
Reproduce the scenario and confirm the error no longer appears in logs.
