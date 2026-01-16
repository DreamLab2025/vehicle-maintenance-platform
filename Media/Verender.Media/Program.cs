using Verender.DatabaseMigrationHelpers;
using Verender.Media.Bootstrapping;
using Verender.Media.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<MediaDbContext>();

app.UseApplicationServices();

app.Run();
