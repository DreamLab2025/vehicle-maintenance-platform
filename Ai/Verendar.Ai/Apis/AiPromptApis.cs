using Verendar.Ai.Domain.Enums;
using Verendar.Common.EndpointFilters;

namespace Verendar.Ai.Apis;

public static class AiPromptApis
{
    public static IEndpointRouteBuilder MapAiPromptApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/ai/prompts")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .MapAiPromptRoutes()
            .WithTags("AI Prompts");

        return builder;
    }

    private static RouteGroupBuilder MapAiPromptRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllPrompts)
            .WithName("GetAllPrompts")
            .WithOpenApi(op => { op.Summary = "Lấy tất cả prompt"; return op; })
            .Produces<ApiResponse<List<AiPromptResponse>>>(StatusCodes.Status200OK);

        group.MapGet("/{operation:int}", GetPromptByOperation)
            .WithName("GetPromptByOperation")
            .WithOpenApi(op => { op.Summary = "Lấy prompt theo operation"; return op; })
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{operation:int}", UpdatePrompt)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateAiPromptRequest>())
            .WithName("UpdatePrompt")
            .WithOpenApi(op => { op.Summary = "Cập nhật nội dung prompt"; return op; })
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("/{operation:int}/versions", GetPromptVersions)
            .WithName("GetPromptVersions")
            .WithOpenApi(op => { op.Summary = "Lịch sử version của prompt"; return op; })
            .Produces<ApiResponse<List<AiPromptVersionResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<AiPromptVersionResponse>>>(StatusCodes.Status404NotFound);

        group.MapPost("/{operation:int}/rollback", RollbackPrompt)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<RollbackAiPromptRequest>())
            .WithName("RollbackPrompt")
            .WithOpenApi(op => { op.Summary = "Rollback prompt về version cũ"; return op; })
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAllPrompts(
        IAiPromptService promptService,
        CancellationToken cancellationToken)
    {
        var result = await promptService.GetAllAsync(cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPromptByOperation(
        int operation,
        IAiPromptService promptService,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(AiOperation), operation))
            return Results.BadRequest(ApiResponse<AiPromptResponse>.FailureResponse("Operation không hợp lệ"));

        var result = await promptService.GetByOperationAsync((AiOperation)operation, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdatePrompt(
        int operation,
        UpdateAiPromptRequest request,
        IAiPromptService promptService,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(AiOperation), operation))
            return Results.BadRequest(ApiResponse<AiPromptResponse>.FailureResponse("Operation không hợp lệ"));

        var result = await promptService.UpdateAsync((AiOperation)operation, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPromptVersions(
        int operation,
        IAiPromptService promptService,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(AiOperation), operation))
            return Results.BadRequest(ApiResponse<List<AiPromptVersionResponse>>.FailureResponse("Operation không hợp lệ"));

        var result = await promptService.GetVersionsAsync((AiOperation)operation, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RollbackPrompt(
        int operation,
        RollbackAiPromptRequest request,
        IAiPromptService promptService,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(AiOperation), operation))
            return Results.BadRequest(ApiResponse<AiPromptResponse>.FailureResponse("Operation không hợp lệ"));

        var result = await promptService.RollbackAsync((AiOperation)operation, request, cancellationToken);
        return result.ToHttpResult();
    }
}
