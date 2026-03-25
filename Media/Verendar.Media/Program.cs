using Verendar.DatabaseMigrationHelpers;
using Verendar.Media.Bootstrapping;
using Verendar.Media.Infrastructure.Data;
using Verendar.Media.Infrastructure.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<MediaDbContext>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await OdometerTestMediaSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();
