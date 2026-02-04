using Verendar.DatabaseMigrationHelpers;
using Verendar.Identity.Bootstrapping;
using Verendar.Identity.Data;
using Verendar.Identity.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<UserDbContext>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await TestUserSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();
