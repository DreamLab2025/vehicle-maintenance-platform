using VMP.DatabaseMigrationHelpers;
using VMP.Media.Bootstrapping;
using VMP.Media.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<MediaDbContext>();

app.UseApplicationServices();

app.Run();
