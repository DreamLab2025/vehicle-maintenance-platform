using Verendar.DatabaseMigrationHelpers;
using Verendar.Identity.Bootstrapping;
using Verendar.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<UserDbContext>();

app.UseApplicationServices();

app.Run();
