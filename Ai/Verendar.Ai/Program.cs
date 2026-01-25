using Verendar.Ai.Bootstrapping;
using Verendar.Ai.Infrastructure.Data;
using Verendar.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<AiDbContext>();

app.UseApplicationServices();

app.Run();
