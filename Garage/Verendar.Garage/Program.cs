var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<GarageDbContext>();

app.UseApplicationServices();

app.MapGarageApis();

await app.RunAsync();
