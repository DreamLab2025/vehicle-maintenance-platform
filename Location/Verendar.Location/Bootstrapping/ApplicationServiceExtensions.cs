namespace Verendar.Location.Bootstrapping;

using Verendar.Location.Application.Services.Implements;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Apis;
using Verendar.Location.Infrastructure.Data;
using Verendar.Location.Infrastructure.Repositories.Implements;
using Verendar.Location.Domain.Repositories.Interfaces;
using Verendar.Common.Bootstrapping;
using Verendar.ServiceDefaults;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        const string locationDb = "location-db";
        builder.AddPostgresDatabase<LocationDbContext>(locationDb);

        var serviceName = "location-service";
        builder.AddServiceRedis(serviceName);

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // UnitOfWork
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        builder.Services.AddScoped<IProvinceService, ProvinceService>();
        builder.Services.AddScoped<IWardService, WardService>();
        builder.Services.AddScoped<IAdministrativeUnitService, AdministrativeUnitService>();
        builder.Services.AddScoped<IAdministrativeRegionService, AdministrativeRegionService>();

        return builder;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapLocationApi();
        app.MapInternalLocationApi();

        return app;
    }
}
