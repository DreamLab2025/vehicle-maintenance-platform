using FluentValidation;
using Hangfire;
using QuestPDF.Infrastructure;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Http;
using Verendar.ServiceDefaults;
using Verendar.Vehicle.Application.Validators;
using Verendar.Vehicle.Apis;
using Verendar.Vehicle.Application.Services.Implements;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Infrastructure.Services;
using Verendar.Vehicle.Infrastructure.Clients;
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
            QuestPDF.Settings.License = LicenseType.Community;

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
                    ?? builder.Configuration["Services:Identity:BaseUrl"];
                client.BaseAddress = new Uri(string.IsNullOrEmpty(baseUrl)
                    ? "https+http://identity-service"
                    : baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddScoped<OdometerReminderJob>();
            builder.Services.AddScoped<MaintenanceReminderJob>();

            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddScoped<ITypeService, TypeService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<IModelService, ModelService>();
            builder.Services.AddScoped<IVariantService, VariantService>();
            builder.Services.AddScoped<IUserVehicleService, UserVehicleService>();
            builder.Services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
            builder.Services.AddScoped<IMaintenanceReminderLookupService, MaintenanceReminderLookupService>();
            builder.Services.AddScoped<IOdometerHistoryService, OdometerHistoryService>();
            builder.Services.AddScoped<IPartTrackingService, PartTrackingService>();
            builder.Services.AddScoped<IDefaultScheduleService, DefaultScheduleService>();
            builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
            builder.Services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();
            builder.Services.AddScoped<IMaintenanceExportService, MaintenanceExportService>();
            builder.Services.AddScoped<IMaintenanceProposalService, MaintenanceProposalService>();

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<UserVehicleRequestValidator>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.UseCommonService();

            if (app.Environment.IsDevelopment())
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = Array.Empty<IDashboardAuthorizationFilter>()
                });
            }

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
            app.MapVariantApi();
            app.MapUserVehicleApi();
            app.MapOdometerHistoryApi();
            app.MapModelScheduleApi();
            app.MapMaintenanceRecordApi();
            app.MapPartCategoryApi();
            app.MapMaintenanceProposalApi();
            app.MapInternalVehicleApi();
            app.MapInternalGarageVehicleApi();
            app.MapInternalMaintenanceReminderApi();

            return app;
        }
    }
}
