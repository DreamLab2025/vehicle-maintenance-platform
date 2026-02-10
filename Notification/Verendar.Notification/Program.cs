using Verendar.DatabaseMigrationHelpers;
using Verendar.Notification.Bootstrapping;
using Verendar.Notification.Infrastructure.Data;
using Verendar.Notification.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<NotificationDbContext>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await EmailTemplateSeeder.SeedAsync(db, logger);
    await NotificationPreferenceSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.Run();
