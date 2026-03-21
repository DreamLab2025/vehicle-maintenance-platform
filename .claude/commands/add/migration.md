Add an EF Core migration for the following change: $ARGUMENTS

## Steps

1. Verify the entity/configuration change is already in place (Domain entity + Infrastructure EF config)
2. Run the migration command from the **repo root**:
   ```bash
   dotnet ef migrations add <MigrationName> \
     --project {Service}/Verendar.{Service}.Infrastructure \
     --startup-project {Service}/Verendar.{Service}
   ```
   Example for Vehicle service:
   ```bash
   dotnet ef migrations add AddTrackingCycleTable \
     --project Vehicle/Verendar.Vehicle.Infrastructure \
     --startup-project Vehicle/Verendar.Vehicle
   ```
3. Review the generated migration file — confirm `Up()` and `Down()` are correct
4. Check for unintended table/column drops
5. Verify partial indexes use `HasFilter("\"DeletedAt\" IS NULL")` for soft-deleted entities
6. Verify global query filter `HasQueryFilter(e => e.DeletedAt == null)` is set in `OnModelCreating`

## Services

| Service      | Infrastructure project                      | Host project                  |
| ------------ | ------------------------------------------- | ----------------------------- |
| Identity     | `Identity/Verendar.Identity.Infrastructure` | `Identity/Verendar.Identity`  |
| Vehicle      | `Vehicle/Verendar.Vehicle.Infrastructure`   | `Vehicle/Verendar.Vehicle`    |
| Media        | `Media/Verendar.Media.Infrastructure`       | `Media/Verendar.Media`        |
| Notification | `Notification/Verendar.Notification.Infrastructure` | `Notification/Verendar.Notification` |
| Ai           | `Ai/Verendar.Ai.Infrastructure`             | `Ai/Verendar.Ai`              |

## Naming convention

PascalCase, descriptive of the schema change:
- `AddTrackingCycleTable`
- `AddEmailVerifiedColumnToUsers`
- `RemoveStatusFromPartCategory`

Do not delete or modify existing migrations.
