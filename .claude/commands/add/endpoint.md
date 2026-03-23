Add a new Minimal API endpoint to the Verendar backend: $ARGUMENTS

Read an existing endpoint in `{Service}/Verendar.{Service}/Apis/` as a pattern before writing.

## Steps

1. **Request DTO** — `{Service}/Verendar.{Service}.Application/Dtos/`; validator in `{Service}/Verendar.{Service}.Application/Validators/`
2. **Response DTO** — `{Service}/Verendar.{Service}.Application/Dtos/`; add `ToResponse()` extension in `{Service}/Verendar.{Service}.Application/Mappings/`
3. **Service method** — add to relevant service in `{Service}/Verendar.{Service}.Application/Services/`; include auth/ownership checks
4. **Repository method** — if new query needed, add interface in `Domain/Repositories/Interfaces/`, implementation in `Infrastructure/Repositories/Implements/`
5. **Endpoint** — add handler to `{Service}/Verendar.{Service}/Apis/`; register in `Map*Routes()` or `Map*Endpoints()` in the same file

For route patterns, auth, pagination, and status codes — see `.claude/skills/backend-development/references/api-design.md`.
