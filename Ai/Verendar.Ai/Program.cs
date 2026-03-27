using Verendar.Ai.Bootstrapping;
using Verendar.Ai.Infrastructure.Data;
using Verendar.DatabaseMigrationHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<AiDbContext>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AiDbContext>();
    await AiPromptSeeder.SeedAsync(context);
}

app.UseApplicationServices();

app.Run();
