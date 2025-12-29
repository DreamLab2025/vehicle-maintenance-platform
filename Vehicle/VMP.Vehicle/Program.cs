using VMP.DatabaseMigrationHelpers;
using VMP.Vehicle.Bootstrapping;
using VMP.Vehicle.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<VehicleDbContext>();

app.UseApplicationServices();

app.Run();