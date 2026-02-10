using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Shared;
using Verendar.ServiceDefaults;
using Verendar.Vehicle.Application.Validators;
using Verendar.Vehicle.Apis;
using Verendar.Vehicle.Application.Services.Implements;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Clients;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;
using Verendar.Vehicle.Infrastructure.Repositories.Implements;
using Verendar.Vehicle.Jobs;

namespace Verendar.Vehicle.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.AddCommonService();

            builder.AddPostgresDatabase<VehicleDbContext>(Const.VehicleDatabase);

            var connectionString = builder.Configuration.GetConnectionString(Const.VehicleDatabase)
                ?? throw new InvalidOperationException($"Connection string '{Const.VehicleDatabase}' not found.");

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

            builder.Services.AddHangfireServer();

            builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["Identity:BaseUrl"]
                    ?? builder.Configuration["Services:Identity:BaseUrl"]
                    ?? "https://localhost:8001";
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddScoped<OdometerReminderJob>();
            builder.Services.AddScoped<MaintenanceReminderJob>();
            builder.Services.AddScoped<IMaintenanceReminderService, Services.MaintenanceReminderService>();

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
            builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
            builder.Services.AddScoped<IVehicleVariantService, VehicleVariantService>();
            builder.Services.AddScoped<IUserVehicleService, UserVehicleService>();
            builder.Services.AddScoped<IDefaultMaintenanceScheduleService, DefaultMaintenanceScheduleService>();
            builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
            builder.Services.AddScoped<IPartProductService, PartProductService>();

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<UserVehicleRequestValidator>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.UseCommonService();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = Array.Empty<IDashboardAuthorizationFilter>()
            });

            RecurringJob.AddOrUpdate<OdometerReminderJob>(
                "odometer-reminder",
                x => x.ExecuteAsync(CancellationToken.None),
                "0 0 * * *");

            RecurringJob.AddOrUpdate<MaintenanceReminderJob>(
                "maintenance-reminder-Critical",
                x => x.ExecuteAsync(CancellationToken.None),
                "0 0 * * *");

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
            app.MapOdometerHistoryApi();
            app.MapDefaultMaintenanceScheduleApi();
            app.MapPartApi();

            return app;
        }
    }
}
