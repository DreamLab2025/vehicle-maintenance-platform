using Verendar.DatabaseMigrationHelpers;
using Verendar.Vehicle.Bootstrapping;
using Verendar.Vehicle.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<VehicleDbContext>();

app.UseApplicationServices();

app.Run();