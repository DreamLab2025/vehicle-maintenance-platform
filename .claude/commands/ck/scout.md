Scout the codebase to locate specific code, files, or patterns: $ARGUMENTS

## Mode
Read-only. No edits. Fast, targeted — not a full exploration.

## How to scout

1. **Glob** for filename patterns (Verendar uses per-service folders):
   - Entities: `{Service}/Verendar.{Service}.Domain/Entities/`
   - Services: `{Service}/Verendar.{Service}.Application/Services/`
   - Endpoints: `{Service}/Verendar.{Service}/Apis/`
   - Validators: `{Service}/Verendar.{Service}.Application/Validators/`
   - Repos: `{Service}/Verendar.{Service}.Infrastructure/Repositories/`
   - Events/Contracts: `{Service}/Verendar.{Service}.Contracts/`

   Services: `Ai` · `Garage` · `Identity` · `Location` · `Media` · `Notification` · `Payment` · `Vehicle`

2. **Grep** for class name, method name, or string literal across the repo root

3. **Follow references** — entity → repo interface → impl → service → endpoint

## Output
- **Found**: file path(s) and line number(s)
- **Structure**: 1–2 sentences per file describing what it does
- **Related**: closely related files (e.g. validator for a DTO, mapping extension for an entity)
