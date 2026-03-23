using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Infrastructure.Clients;
using Verendar.Garage.Infrastructure.Configuration;
using Verendar.Garage.Infrastructure.ExternalServices;
using Verendar.Garage.Infrastructure.ExternalServices.Geocoding;

namespace Verendar.Garage.Bootstrapping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddCommonService();

        builder.AddPostgresDatabase<GarageDbContext>(Const.GarageDatabase);

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IGarageService, GarageService>();
        builder.Services.AddScoped<IGarageBranchService, GarageBranchService>();

        builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Payment:BaseUrl"];
            if (!string.IsNullOrEmpty(baseAddress))
                client.BaseAddress = new Uri(baseAddress);
        });

        builder.Services.AddHttpClient();

        builder.Services.Configure<VietQRSettings>(
            builder.Configuration.GetSection(VietQRSettings.SectionName));
        builder.Services.Configure<GeocodingSettings>(
            builder.Configuration.GetSection(GeocodingSettings.SectionName));

        builder.Services.AddScoped<IBusinessLookupService, VietQRBusinessLookupService>();

        var geocodingApiKey = builder.Configuration.GetSection(GeocodingSettings.SectionName)["ApiKey"];
        if (!string.IsNullOrWhiteSpace(geocodingApiKey))
            builder.Services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();
        else
            builder.Services.AddScoped<IGeocodingService, NominatimGeocodingService>();

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
