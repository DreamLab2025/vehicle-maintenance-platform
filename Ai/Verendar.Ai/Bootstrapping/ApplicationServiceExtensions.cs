using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Apis;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.Data;
using Verendar.Ai.Infrastructure.ExternalServices;
using Verendar.Ai.Infrastructure.Repositories.Implements;
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

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IGenerativeAiService, GeminiService>();
            builder.Services.AddScoped<IVehicleMaintenanceAnalysisService, VehicleMaintenanceAnalysisService>();

            return builder;
        }

        public static WebApplication UseApplicationServices(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.UseCommonService();

            app.UseHttpsRedirection();

            app.MapVehicleQuestionnaireApi();

            return app;
        }
    }
}