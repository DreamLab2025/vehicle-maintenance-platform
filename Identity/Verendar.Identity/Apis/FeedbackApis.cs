using Microsoft.AspNetCore.Mvc;
using Verendar.Common.EndpointFilters;

namespace Verendar.Identity.Apis;

public static class FeedbackApis
{
    public static IEndpointRouteBuilder MapFeedbackApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/feedback")
            .MapFeedbackRoutes()
            .WithTags("Feedback Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapFeedbackRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/", SubmitFeedback)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateFeedbackRequest>())
            .WithName("SubmitFeedback")
            .RequireAuthorization()
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/", GetAllFeedback)
            .WithName("GetAllFeedback")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<List<FeedbackResponse>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetFeedback)
            .WithName("GetFeedbackById")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", UpdateFeedbackStatus)
            .WithName("UpdateFeedbackStatus")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<FeedbackResponse>>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> SubmitFeedback(
        [FromBody] CreateFeedbackRequest request,
        ICurrentUserService currentUserService,
        IFeedbackService feedbackService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await feedbackService.SubmitAsync(userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAllFeedback(
        [AsParameters] PaginationRequest pagination,
        IFeedbackService feedbackService,
        CancellationToken ct)
    {
        var result = await feedbackService.GetAllAsync(pagination, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetFeedback(
        [FromRoute] Guid id,
        IFeedbackService feedbackService,
        CancellationToken ct)
    {
        var result = await feedbackService.GetByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateFeedbackStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateFeedbackStatusRequest request,
        IFeedbackService feedbackService,
        CancellationToken ct)
    {
        var result = await feedbackService.UpdateStatusAsync(id, request, ct);
        return result.ToHttpResult();
    }
}
