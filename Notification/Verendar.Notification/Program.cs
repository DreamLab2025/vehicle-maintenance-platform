using Verendar.DatabaseMigrationHelpers;
using Verendar.Notification.Bootstrapping;
using Verendar.Notification.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<NotificationDbContext>();

app.UseApplicationServices();

app.Run();
