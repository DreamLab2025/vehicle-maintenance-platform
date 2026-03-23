namespace Verendar.Garage.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddCommonService();

        builder.AddPostgresDatabase<GarageDbContext>(Const.GarageDatabase);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        return app;
    }
}
