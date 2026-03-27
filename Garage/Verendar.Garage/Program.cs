var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

await app.MigrateDbContextAsync<GarageDbContext>();

app.UseApplicationServices();

app.MapGarageApi();
app.MapGarageBranchApi();
app.MapGarageMemberApi();
app.MapBookingApi();
app.MapServiceCategoryApi();
app.MapGarageProductApi();
app.MapGarageServiceApi();
app.MapGarageBundleApi();

await app.RunAsync();
