Scout the codebase to locate specific code, files, or patterns: $ARGUMENTS

## Mode
Read-only. No edits. Fast, targeted search — not a full exploration.

## How to scout

1. **Start with a glob** — if you know a filename pattern or directory:
   - Entities: `{Service}/Verendar.{Service}.Domain/Entities/`
   - Services: `{Service}/Verendar.{Service}.Application/Services/`
   - Endpoints: `{Service}/Verendar.{Service}/Apis/`
   - Validators: `{Service}/Verendar.{Service}.Application/Validators/`
   - Repositories: `{Service}/Verendar.{Service}.Infrastructure/Repositories/`

   Services: `Identity`, `Vehicle`, `Garage`, `Media`, `Notification`, `Payment`, `Location`, `Ai`

2. **Search by keyword** — if you're looking for a class name, method, or string literal, grep for it across the service directories.

3. **Follow references** — if you find one piece, read it and trace to the next: entity → repository interface → implementation → service → endpoint.

## Output

Report:
- **Found**: file path(s) and line number(s) where the target lives
- **Structure**: brief description of what the code does (1–2 sentences per file)
- **Related**: other files closely related to the target (e.g. the validator for a DTO, the mapping extension for an entity)

Do not suggest changes. Just report what you found and where.
