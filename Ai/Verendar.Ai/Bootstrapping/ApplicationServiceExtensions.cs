using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Verendar.Ai.Application.Validators;
using Verendar.Ai.Apis;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Common.Http;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Clients;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Ai.Infrastructure.ExternalServices;
using Verendar.Ai.Infrastructure.Repositories.Implements;
using Verendar.Ai.Infrastructure.Services;
using Verendar.Common.Bootstrapping;
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

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAiUsageService, AiUsageService>();

            builder.Services.AddScoped<GeminiService>();
            builder.Services.AddScoped<BedrockService>();
            builder.Services.AddScoped<IGenerativeAiServiceFactory, GenerativeAiServiceFactory>();
            builder.Services.AddScoped<IGenerativeAiService>(sp =>
                sp.GetRequiredService<IGenerativeAiServiceFactory>().CreateDefault());

            builder.Services.AddScoped<IVehicleMaintenanceAnalysisService, VehicleMaintenanceAnalysisService>();
            builder.Services.AddScoped<IOdometerScanService, OdometerScanService>();
            builder.Services.AddScoped<IAiUsageAnalyticsService, AiUsageAnalyticsService>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();
            app.UseCommonService();
            app.UseHttpsRedirection();
            app.MapAiApi();

            return app;
        }
    }
}
