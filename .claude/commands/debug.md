Investigate and fix the following issue in the Verendar backend: $ARGUMENTS

## Investigation
1. Reproduce — what input triggers it, actual vs expected behavior
2. Trace: API route handler → service → repository → DB
3. Check JWT/role if 401/403
4. Check EF Core query if wrong/missing data
5. Check FluentValidation if 400
6. Check MassTransit consumer if async event issue
7. Check external integration if VNPay / Google Maps / Gemini involved

## Diagnosis format
- Root cause (specific — which query/method and why)
- Why it went undetected
- Fix

## Fix
- Minimal change — don't refactor surrounding code
- Follow layer rules: Domain → Application → Infrastructure → Host
- If DB query changes, check for N+1
