using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Health;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;

namespace Verendar.Ai.Apis
{
    public static class AIApis
    {
        public static IEndpointRouteBuilder MapAiApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/ai")
                .MapAiRoutes()
                .WithTags("AI");

            return builder;
        }

        public static RouteGroupBuilder MapAiRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/health", GetHealth)
                .WithName("HealthCheck")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Health check";
                    operation.Description = "Kiểm tra trạng thái kết nối của AI service với third-party (Gemini/Bedrock).";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<HealthCheckResponse>(StatusCodes.Status200OK);

            return group;
        }

        private static async Task<IResult> GetHealth(
            IGenerativeAiService aiService,
            IOptions<AiProviderOptions> providerOptions,
            CancellationToken cancellationToken)
        {
            var provider = string.IsNullOrWhiteSpace(providerOptions.Value.Provider) ? "Gemini" : providerOptions.Value.Provider.Trim();
            var (success, errorMessage) = await aiService.CheckConnectivityAsync(cancellationToken);

            var response = new HealthCheckResponse
            {
                Status = success ? "Healthy" : "Unhealthy",
                Provider = provider,
                ThirdPartyAiConnected = success,
                Message = success ? null : errorMessage
            };

            return Results.Ok(response);
        }
    }
}
