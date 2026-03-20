Add a new Minimal API endpoint to the Verendar backend: $ARGUMENTS

Read an existing endpoint in the target service's `Routes/` folder as a pattern before writing.

## Steps

1. **Request DTO** — `{Service}/Verendar.{Service}.Application/DTOs/`; validator in `Validators/`
2. **Response DTO** — same folder; add `ToResponse()` static extension in the entity or DTO file
3. **Service method** — add to relevant service in `Application/Services/`
4. **Repository method** — if new query needed, add interface in `Domain/Repositories/`, impl in `Infrastructure/`
5. **Endpoint** — add private handler + register in `Map*Routes()` in `{Service}/Verendar.{Service}/Routes/`

For route patterns, auth, pagination, status codes — see `.claude/skills/backend-development/references/api-design.md`.
