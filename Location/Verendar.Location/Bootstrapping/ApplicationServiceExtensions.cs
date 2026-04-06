using Amazon.GeoPlaces;
using Amazon.Runtime;
using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Infrastructure.Configuration;
using Verendar.Location.Infrastructure.ExternalServices.Geocoding;
using Verendar.Location.Infrastructure.ExternalServices.PlaceSearch;

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

        builder.Services.Configure<AwsGeocodingSettings>(
            builder.Configuration.GetSection(AwsGeocodingSettings.SectionName));

        var awsGeoEnabled = builder.Configuration.GetValue("Geocoding:AWS:Enabled", true);
        if (awsGeoEnabled)
        {
            var awsOptions = builder.Configuration.GetAWSOptions("Geocoding:AWS");
            var accessKey = builder.Configuration["Geocoding:AWS:AccessKey"];
            var secretKey = builder.Configuration["Geocoding:AWS:SecretKey"];
            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
            builder.Services.AddDefaultAWSOptions(awsOptions);
            builder.Services.AddAWSService<IAmazonGeoPlaces>();
            builder.Services.AddScoped<IGeocodingService, AwsGeocodingService>();
            builder.Services.AddScoped<IPlaceSearchService, AwsPlaceSearchService>();
        }
        else
        {
            builder.Services.AddScoped<IGeocodingService, NullGeocodingService>();
            builder.Services.AddScoped<IPlaceSearchService, NullPlaceSearchService>();
        }

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();

        app.UseCommonService();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapLocationApi();
        app.MapInternalLocationApi();

        return app;
    }
}
