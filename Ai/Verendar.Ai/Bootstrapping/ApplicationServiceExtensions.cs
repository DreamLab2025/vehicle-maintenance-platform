using Microsoft.Extensions.Options;
using Verendar.Ai.Apis;
using Verendar.Ai.Application.Handlers;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Ai.Infrastructure.ExternalServices;
using Verendar.Ai.Infrastructure.Repositories.Implements;
using Verendar.Ai.Infrastructure.Services;
using Verendar.Common.Bootstrapping;
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

            builder.Services.AddHttpClient();
            builder.Services.Configure<GeminiSettings>(
                builder.Configuration.GetSection(GeminiSettings.SectionName));
            builder.Services.Configure<BedrockSettings>(
                builder.Configuration.GetSection(BedrockSettings.SectionName));
            builder.Services.Configure<AiProviderOptions>(options =>
            {
                options.Provider = builder.Configuration.GetValue<string>(AiProviderOptions.ConfigKey) ?? "Gemini";
            });

            builder.AddClients();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAiUsageService, AiUsageService>();

            builder.Services.AddScoped<GeminiService>();
            builder.Services.AddScoped<BedrockService>();
            builder.Services.AddScoped<IGenerativeAiService>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
                var isBedrock = (options.Provider ?? "Gemini").Trim().Equals("Bedrock", StringComparison.OrdinalIgnoreCase);

                IGenerativeAiService inner = isBedrock
                    ? sp.GetRequiredService<BedrockService>()
                    : sp.GetRequiredService<GeminiService>();

                var provider = isBedrock ? AiProvider.Bedrock : AiProvider.Gemini;
                return new AiUsageTrackingDecorator(inner, sp.GetRequiredService<IAiUsageService>(), provider);
            });

            builder.Services.AddScoped<IVehicleMaintenanceAnalysisService, VehicleMaintenanceAnalysisService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ForwardAuthorizationHandler>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();
            app.UseCommonService();
            app.UseHttpsRedirection();
            app.MapVehicleQuestionnaireApi();
            app.MapAiApi();

            return app;
        }
    }
}
