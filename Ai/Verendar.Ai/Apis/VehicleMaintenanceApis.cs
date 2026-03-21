using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
namespace Verendar.Ai.Apis
{
    public static class VehicleMaintenanceApis
    {
        public static IEndpointRouteBuilder MapVehicleQuestionnaireApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/ai/vehicle-questionnaire")
                .MapVehicleQuestionnaireRoutes()
                .WithTags("Vehicle Questionnaire Analysis Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapVehicleQuestionnaireRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/analyze", AnalyzeQuestionnaire)
                .WithName("AnalyzeQuestionnaire")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Phân tích câu hỏi về xe và đưa ra khuyến nghị bảo dưỡng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<VehicleQuestionnaireResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VehicleQuestionnaireResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

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
            {
                return Results.Unauthorized();
            }

            var result = await analysisService.AnalyzeQuestionnaireAsync(userId, request, cancellationToken);

            return result.ToHttpResult();
        }
    }
}
