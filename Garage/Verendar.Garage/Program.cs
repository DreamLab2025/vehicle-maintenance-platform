using Verendar.Garage.Infrastructure.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<GarageDbContext>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GarageDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await GarageBranchDevSeeder.SeedAsync(db, logger);
}

app.UseApplicationServices();

app.MapGarageApi();
app.MapGarageBranchApi();
app.MapGarageMemberApi();
app.MapBookingApi();
app.MapServiceCategoryApi();
app.MapGarageProductApi();
app.MapGarageServiceApi();
app.MapGarageBundleApi();
app.MapReviewApi();
app.MapStatsApi();

await app.RunAsync();
