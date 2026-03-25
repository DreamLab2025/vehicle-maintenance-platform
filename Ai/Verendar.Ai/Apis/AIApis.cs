using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Health;
using Verendar.Ai.Application.Dtos.OdometerScan;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Common.EndpointFilters;

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

        private static RouteGroupBuilder MapAiRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/vehicle-questionnaire/analyze", AnalyzeQuestionnaire)
                .WithName("AnalyzeQuestionnaire")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Phân tích câu hỏi về xe và đưa ra khuyến nghị bảo dưỡng";
                    return operation;
                })
                .RequireAuthorization()
                .RequireRateLimiting("Fixed")
                .Produces<ApiResponse<VehicleQuestionnaireResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VehicleQuestionnaireResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/odometer-scans", ScanOdometer)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<OdometerScanRequest>())
                .WithName("ScanOdometer")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đọc số km từ ảnh đồng hồ xe";
                    operation.Description = "Upload ảnh chụp đồng hồ công-tơ-mét, AI sẽ nhận diện và trả về số km. User cần xác nhận trước khi lưu.";
                    return operation;
                })
                .RequireAuthorization()
                .RequireRateLimiting("Fixed")
                .Produces<ApiResponse<OdometerScanResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<OdometerScanResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/health", GetHealth)
                .WithName("HealthCheck")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Kiểm tra trạng thái kết nối của AI service với third-party (Gemini/Bedrock).";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<HealthCheckResponse>(StatusCodes.Status200OK);

            return group;
        }

        private static async Task<IResult> AnalyzeQuestionnaire(
            VehicleQuestionnaireRequest request,
            IVehicleMaintenanceAnalysisService analysisService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await analysisService.AnalyzeQuestionnaireAsync(userId, request, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> ScanOdometer(
            OdometerScanRequest request,
            IOdometerScanService odometerScanService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await odometerScanService.ScanOdometerAsync(userId, request, cancellationToken);
            return result.ToHttpResult();
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
