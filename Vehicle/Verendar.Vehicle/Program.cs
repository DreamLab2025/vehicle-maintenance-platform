using Verendar.DatabaseMigrationHelpers;
using Verendar.Vehicle.Bootstrapping;
using Verendar.Vehicle.Infrastructure.Data;
using Verendar.Vehicle.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<VehicleDbContext>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await VehicleCatalogSeeder.SeedAsync(db, logger);
    await PartProductSeeder.SeedAsync(db, logger);
    await MaintenanceReminderTestDataSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();