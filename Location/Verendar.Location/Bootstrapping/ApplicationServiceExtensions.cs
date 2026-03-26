using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Infrastructure.Configuration;
using Verendar.Location.Infrastructure.ExternalServices.Geocoding;

namespace Verendar.Location.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddCommonService();

        builder.AddPostgresDatabase<LocationDbContext>(Const.LocationDatabase);

        builder.AddServiceRedis(nameof(Location), connectionName: Const.Redis);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<IProvinceService, ProvinceService>();
        builder.Services.AddScoped<IWardService, WardService>();
        builder.Services.AddScoped<IAdministrativeUnitService, AdministrativeUnitService>();
        builder.Services.AddScoped<IAdministrativeRegionService, AdministrativeRegionService>();

        builder.Services.Configure<GeocodingSettings>(
            builder.Configuration.GetSection(GeocodingSettings.SectionName));
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();

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

        app.MapLocationApi();
        app.MapInternalLocationApi();

        return app;
    }
}
