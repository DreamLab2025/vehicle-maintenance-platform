using VMP.DatabaseMigrationHelpers;
using VMP.Identity.Bootstrapping;
using VMP.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<UserDbContext>();

app.UseApplicationServices();

app.Run();
