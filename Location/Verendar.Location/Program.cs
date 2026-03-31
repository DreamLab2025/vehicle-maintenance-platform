var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<LocationDbContext>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LocationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await LocationCatalogSeeder.SeedAsync(db, logger);
    await ProvinceBoundaryUrlSeeder.SeedAsync(db, logger);
    await WardBoundaryUrlSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

await app.RunAsync();
