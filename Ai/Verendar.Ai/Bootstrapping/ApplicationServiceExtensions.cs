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

            builder.Services.Configure<GeminiSettings>(
                builder.Configuration.GetSection(GeminiSettings.SectionName));

            // Read timeout configuration - ensure we get 180 seconds (3 minutes) for AI service
            var geminiSection = builder.Configuration.GetSection(GeminiSettings.SectionName);
            var timeoutSeconds = geminiSection.GetValue<int>("TimeoutSeconds", 180);
            var maxRetries = geminiSection.GetValue<int>("MaxRetries", 3);
            
            // Force minimum 180 seconds (3 minutes) for AI service
            if (timeoutSeconds < 180)
            {
                timeoutSeconds = 180;
            }
            
            // Log configuration for debugging
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger("Verendar.Ai.Bootstrapping");
            logger.LogInformation("=== Gemini HttpClient Configuration ===");
            logger.LogInformation("TimeoutSeconds: {TimeoutSeconds} seconds ({Minutes} minutes)", timeoutSeconds, timeoutSeconds / 60);
            logger.LogInformation("MaxRetries: {MaxRetries}", maxRetries);
            
            // Configure named HttpClient for Gemini with custom resilience policies
            // IMPORTANT: Named clients bypass ConfigureHttpClientDefaults, so we must configure explicitly
            builder.Services.AddHttpClient("Gemini", client =>
            {
                // HttpClient timeout should be longer than the resilience timeout to avoid conflicts
                var httpClientTimeout = timeoutSeconds + 30; // Add extra buffer
                client.Timeout = TimeSpan.FromSeconds(httpClientTimeout);
                logger.LogInformation("HttpClient.Timeout: {Timeout} seconds", httpClientTimeout);
            })
            .AddStandardResilienceHandler(options =>
            {
                // Configure attempt timeout (timeout for each retry attempt)
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                logger.LogInformation("AttemptTimeout: {Timeout} seconds", timeoutSeconds);
                
                // Configure total request timeout (overall timeout for the entire request)
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                logger.LogInformation("TotalRequestTimeout: {Timeout} seconds", timeoutSeconds);
                
                // Configure retry policy
                options.Retry.MaxRetryAttempts = maxRetries;
                logger.LogInformation("MaxRetryAttempts: {MaxRetries}", maxRetries);
                
                // Configure circuit breaker: sampling duration must be at least double the attempt timeout
                var samplingDuration = timeoutSeconds * 2;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(samplingDuration);
                logger.LogInformation("CircuitBreaker.SamplingDuration: {Duration} seconds", samplingDuration);
                
                logger.LogInformation("=== End Gemini Configuration ===");
            });

            builder.Services.AddHttpClient();

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

            app.MapVehicleMaintenanceApi();

            return app;
        }
    }
}