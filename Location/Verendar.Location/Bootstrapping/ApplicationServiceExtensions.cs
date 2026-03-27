using Amazon.GeoPlaces;
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

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<AwsGeocodingSettings>(
                builder.Configuration.GetSection(AwsGeocodingSettings.SectionName));

            var awsGeoEnabled = builder.Configuration.GetValue("Geocoding:AWS:Enabled", true);
            if (awsGeoEnabled)
            {
                builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("Geocoding:AWS"));
                builder.Services.AddAWSService<IAmazonGeoPlaces>();
                builder.Services.AddScoped<IGeocodingService, AwsGeocodingService>();
            }
            else
            {
                builder.Services.AddScoped<IGeocodingService, NullGeocodingService>();
            }
        }
        else
        {
            // Production: Google Maps
            builder.Services.Configure<GoogleGeocodingSettings>(
                builder.Configuration.GetSection(GoogleGeocodingSettings.SectionName));
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();
        }

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
