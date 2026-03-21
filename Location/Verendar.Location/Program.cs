using Verendar.Location.Bootstrapping;
using Verendar.Location.Infrastructure.Data;
using Verendar.Location.Infrastructure.Data.Seeders;
using Verendar.ServiceDefaults;
using Verendar.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<LocationDbContext>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LocationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await LocationCatalogSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

await app.RunAsync();
