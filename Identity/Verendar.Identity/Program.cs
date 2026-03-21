using Verendar.DatabaseMigrationHelpers;
using Verendar.Identity.Bootstrapping;
using Verendar.Identity.Infrastructure.Data;
using Verendar.Identity.Infrastructure.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<UserDbContext>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await TestUserSeeder.SeedAsync(db, logger);
    await AdminUserSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();
