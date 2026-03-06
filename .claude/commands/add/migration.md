Add an EF Core migration for the following change: $ARGUMENTS

## Steps

1. Identify which service's `DbContext` needs the migration (VehicleDbContext, IdentityDbContext, etc.)
2. Verify the entity/configuration change is already in place in Domain and Infrastructure
3. Run the migration command from the solution root:
   ```bash
   dotnet ef migrations add <MigrationName> --project <Service>/<Service>.Infrastructure --startup-project <Service>/<Service>
   ```
4. Review the generated migration file — confirm `Up()` and `Down()` are correct
5. Check for any unintended table/column drops

Migration naming convention: `PascalCase`, descriptive of what changed (e.g., `AddVehiclePartTrackingAiFields`, `RenameConsumableItemToPartCategory`)

Migrations auto-run on startup via `MigrateDbContextAsync<T>()` — no manual apply needed in dev.

Do not delete or modify existing migrations.
