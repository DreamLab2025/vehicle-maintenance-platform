using VMP.Common.Bootstrapping;
using VMP.Common.Shared;
using VMP.ServiceDefaults;
using VMP.Vehicle.Apis;
using VMP.Vehicle.Application.Services.Implements;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;
using VMP.Vehicle.Infrastructure.Repositories.Implements;

namespace VMP.Vehicle.Bootstrapping
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
            builder.Services.AddScoped<IOilService, OilService>();
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
            app.MapVehiclePartApi();
            app.MapOilApi();
            app.MapSMScheduleApi();
            app.MapMaintenanceActivityApi();
            app.MapOdometerHistoryApi();

            return app;
        }
    }
}
