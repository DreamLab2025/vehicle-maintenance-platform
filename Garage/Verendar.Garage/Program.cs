var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<GarageDbContext>();

app.UseApplicationServices();

await app.RunAsync();
