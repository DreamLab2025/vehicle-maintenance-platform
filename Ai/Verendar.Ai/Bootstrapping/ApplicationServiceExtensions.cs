using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Verendar.Ai.Application.Validators;
using Verendar.Ai.Apis;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Common.Bootstrapping;
using Verendar.Common.Http;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Clients;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Ai.Infrastructure.ExternalServices;
using Verendar.Ai.Infrastructure.Jobs;
using Verendar.Ai.Infrastructure.Repositories.Implements;
using Verendar.Ai.Infrastructure.Services;
using Verendar.Common.Shared;
using Verendar.ServiceDefaults;

namespace Verendar.Ai.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.AddCommonService();
            builder.AddPostgresDatabase<AiDbContext>(Const.AiDatabase);
            builder.AddServiceRedis("ai-service", connectionName: Const.Redis);

            var connectionString = builder.Configuration.GetConnectionString(Const.AiDatabase)
                ?? throw new InvalidOperationException($"Connection string '{Const.AiDatabase}' not found.");

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

            builder.Services.AddHangfireServer();

            builder.Services.AddHttpClient();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ForwardAuthorizationHandler>();

            builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["VehicleService:BaseUrl"]
                    ?? builder.Configuration["Services:Vehicle:BaseUrl"]
                    ?? "https://localhost:8002";
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<ForwardAuthorizationHandler>();

            builder.Services.AddHttpClient<IMediaServiceClient, MediaServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["MediaService:BaseUrl"]
                    ?? builder.Configuration["Services:Media:BaseUrl"]
                    ?? "https://localhost:8003";
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddValidatorsFromAssemblyContaining<OdometerScanRequestValidator>();
            builder.Services.Configure<GeminiSettings>(
                builder.Configuration.GetSection(GeminiSettings.SectionName));
            builder.Services.Configure<BedrockSettings>(
                builder.Configuration.GetSection(BedrockSettings.SectionName));
            builder.Services.Configure<AiProviderOptions>(options =>
            {
                options.Provider = builder.Configuration.GetValue<string>(AiProviderOptions.ConfigKey) ?? "Gemini";
            });
            builder.Services.Configure<PromptVersioningOptions>(
                builder.Configuration.GetSection(PromptVersioningOptions.SectionName));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAiUsageService, AiUsageService>();

            builder.Services.AddScoped<GeminiService>();
            builder.Services.AddScoped<BedrockService>();
            builder.Services.AddScoped<IGenerativeAiServiceFactory, GenerativeAiServiceFactory>();
            builder.Services.AddScoped<IGenerativeAiService>(sp =>
                sp.GetRequiredService<IGenerativeAiServiceFactory>().CreateDefault());

            builder.Services.AddScoped<IAiPromptService, AiPromptService>();
            builder.Services.AddScoped<IVehicleMaintenanceAnalysisService, VehicleMaintenanceAnalysisService>();
            builder.Services.AddScoped<IOdometerScanService, OdometerScanService>();
            builder.Services.AddScoped<IAiUsageAnalyticsService, AiUsageAnalyticsService>();

            builder.Services.AddScoped<AiPromptRetentionJob>();

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

            RecurringJob.AddOrUpdate<AiPromptRetentionJob>(
                "ai-prompt-retention",
                job => job.ExecuteAsync(CancellationToken.None),
                Cron.Daily);

            app.UseHttpsRedirection();
            app.MapAiApi();
            app.MapAiPromptApi();

            return app;
        }
    }
}
