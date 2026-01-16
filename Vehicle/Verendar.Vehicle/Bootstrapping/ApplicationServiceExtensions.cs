using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.ServiceDefaults;
using Verendar.Vehicle.Apis;
using Verendar.Vehicle.Application.Services.Implements;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;
using Verendar.Vehicle.Infrastructure.Repositories.Implements;

namespace Verendar.Vehicle.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<VehicleDbContext>(Const.VehicleDatabase);

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
            builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
            builder.Services.AddScoped<IVehicleVariantService, VehicleVariantService>();
            builder.Services.AddScoped<IUserVehicleService, UserVehicleService>();
            builder.Services.AddScoped<IMaintenanceActivityService, MaintenanceActivityService>();

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

            app.MapBrandApi();
            app.MapTypeApi();
            app.MapModelApi();
            app.MapModelImageApi();
            app.MapUserVehicleApi();
            app.MapConsumableItemApi();
            app.MapSMScheduleApi();
            app.MapMaintenanceActivityApi();
            app.MapOdometerHistoryApi();

            return app;
        }
    }
}
