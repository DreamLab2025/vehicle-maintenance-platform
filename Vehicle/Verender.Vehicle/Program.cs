using Verender.DatabaseMigrationHelpers;
using Verender.Vehicle.Bootstrapping;
using Verender.Vehicle.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<VehicleDbContext>();

app.UseApplicationServices();

app.Run();