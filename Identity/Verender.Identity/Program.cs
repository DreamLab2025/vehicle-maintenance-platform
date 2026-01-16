using Verender.DatabaseMigrationHelpers;
using Verender.Identity.Bootstrapping;
using Verender.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<UserDbContext>();

app.UseApplicationServices();

app.Run();
