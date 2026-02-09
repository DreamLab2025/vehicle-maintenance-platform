using Microsoft.EntityFrameworkCore;
using Verendar.DatabaseMigrationHelpers;
using Verendar.Vehicle.Bootstrapping;
using Verendar.Vehicle.Infrastructure.Data;
using Verendar.Vehicle.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<VehicleDbContext>(async (database, ct) =>
{
  await database.ExecuteSqlRawAsync(
      """
        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'OdometerHistories' AND column_name = 'KmOnRecordedDate'
          ) THEN
            ALTER TABLE "OdometerHistories" ADD COLUMN "KmOnRecordedDate" integer NULL;
          END IF;
        END $$;
        """,
      ct ?? default);
});

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
  var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
  await MaintenanceReminderTestDataSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();