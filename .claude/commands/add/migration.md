Add an EF Core migration for the following change: $ARGUMENTS

## Steps

1. Identify the `ResearchHubDbContext` in `src/ResearchHub.Infrastructure/Persistence/`
2. Verify the entity/configuration change is already in place in Domain and Infrastructure
3. Run the migration command from the solution root:
   ```bash
   dotnet ef migrations add <MigrationName> --project src/ResearchHub.Infrastructure --startup-project src/ResearchHub.Api
   ```
4. Review the generated migration file — confirm `Up()` and `Down()` are correct
5. Check for any unintended table/column drops

Migration naming convention: `PascalCase`, descriptive of what changed (e.g., `AddProjectMembersTable`, `AddSeminarStatusColumn`)

Reference `docs/design/` for the expected table schemas:
- `01-user-auth-tables.md` — User, Role, Permission, OTP
- `02-academic-tables.md` — Department, Major, Course, ResearchQuestion
- `03-project-seminar-tables.md` — Project, SeminarPlan, Members
- `04-semester-file-tables.md` — Semester, FileAttachment

Do not delete or modify existing migrations.
