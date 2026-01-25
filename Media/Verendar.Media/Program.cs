using Verendar.DatabaseMigrationHelpers;
using Verendar.Media.Bootstrapping;
using Verendar.Media.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<MediaDbContext>();

app.UseApplicationServices();

app.Run();
