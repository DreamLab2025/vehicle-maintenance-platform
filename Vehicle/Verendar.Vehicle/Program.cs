using Microsoft.EntityFrameworkCore;
using Verendar.DatabaseMigrationHelpers;
using Verendar.Vehicle.Bootstrapping;
using Verendar.Vehicle.Infrastructure.Data;

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

app.UseApplicationServices();

app.Run();