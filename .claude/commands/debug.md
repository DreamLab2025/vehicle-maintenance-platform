Investigate and fix the following issue in the Verendar backend: $ARGUMENTS

## Investigation approach
1. Reproduce the problem — what exact input triggers it, what is the actual vs expected behavior
2. Trace the call path — from API endpoint → handler/service → repository → DB
3. Check authorization flow if the issue involves 401/403
4. Check EF Core query if the issue involves wrong/missing data
5. Check FluentValidation pipeline if the issue involves 400s
6. Check RabbitMQ consumer if the issue involves async events not triggering
7. Check Hangfire job logs if the issue involves background jobs

## Diagnosis format
- Root cause (be specific — not "the query is wrong" but which query and why)
- Why it went undetected (missing validation? wrong assumption? missing test?)
- Fix

## Fix approach
- Minimal change — don't refactor surrounding code unless it's the root cause
- If the fix touches the domain, follow the layer rules (Domain → Application → Infrastructure → Host)
- If the fix changes a DB query, check for N+1 risk
