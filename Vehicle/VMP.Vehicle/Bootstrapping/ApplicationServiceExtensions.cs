using VMP.Common.Bootstrapping;
using VMP.Common.Shared;
using VMP.ServiceDefaults;
using VMP.Vehicle.Application.Services.Implements;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Infrastructure.Data;
using VMP.Vehicle.Infrastructure.Repositories.Implements;
using VMP.Vehicle.Infrastructure.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.UnitOfWork;

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
            builder.Services.AddScoped<IVehicleUnitOfWork, VehicleUnitOfWork>();

            // Register Repositories
            builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
            builder.Services.AddScoped<IVehicleBrandRepository, VehicleBrandRepository>();
            builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
            builder.Services.AddScoped<IUserVehicleRepository, UserVehicleRepository>();
            builder.Services.AddScoped<IConsumableItemRepository, ConsumableItemRepository>();
            builder.Services.AddScoped<IMaintenanceActivityRepository, MaintenanceActivityRepository>();

            // Register Services
            builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
            builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
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

            return app;
        }
    }
}
