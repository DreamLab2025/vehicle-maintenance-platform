using FluentValidation;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Infrastructure.Clients;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Application.Validators;
using Verendar.Garage.Infrastructure.Configuration;
using Verendar.Garage.Infrastructure.ExternalServices;

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
        builder.Services.AddScoped<IGarageMemberService, GarageMemberService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
        builder.Services.AddScoped<IGarageProductService, GarageProductService>();
        builder.Services.AddScoped<IGarageServiceService, GarageServiceService>();
        builder.Services.AddScoped<IGarageBundleService, GarageBundleService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();
        builder.Services.AddScoped<IStatsService, StatsService>();
        builder.Services.AddScoped<IGarageCatalogService, GarageCatalogService>();

        builder.Services.AddValidatorsFromAssemblyContaining<CreateBookingRequestValidator>();

        builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Payment:BaseUrl"];
            if (!string.IsNullOrEmpty(baseAddress))
                client.BaseAddress = new Uri(baseAddress);
        });

        builder.Services.AddHttpClient<ILocationClient, LocationHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Location:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
                ? "https+http://location-service"
                : baseAddress);
        })
        .AddServiceDiscovery();

        builder.Services.AddHttpClient<IIdentityClient, IdentityHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Identity:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
                ? "https+http://identity-service"
                : baseAddress);
        })
        .AddServiceDiscovery();

        builder.Services.AddHttpClient<IGarageIdentityContactClient, GarageIdentityContactHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Identity:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
                ? "https+http://identity-service"
                : baseAddress);
        })
        .AddServiceDiscovery();

        builder.Services.AddHttpClient<IVehicleGarageClient, VehicleGarageHttpClient>(client =>
        {
            var baseAddress = builder.Configuration["Services:Vehicle:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
                ? "https+http://vehicle-service"
                : baseAddress);
        })
        .AddServiceDiscovery();

        builder.Services.AddHttpClient();

        builder.Services.Configure<VietQRSettings>(
            builder.Configuration.GetSection(VietQRSettings.SectionName));

        builder.Services.AddScoped<IBusinessLookupService, VietQRBusinessLookupService>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole(RoleType.Admin.ToString()));
        });

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
