using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;

namespace Verendar.Ai.Apis;

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
                operation.Description = "Endpoint này nhận thông tin xe, lịch bảo dưỡng chuẩn, " +
                                      "và câu trả lời của người dùng. AI sẽ phân tích và đưa ra " +
                                      "khuyến nghị tracking chính xác cho từng linh kiện.";
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
        request.UserId = currentUserService.UserId;

        var result = await analysisService.AnalyzeQuestionnaireAsync(request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
}
