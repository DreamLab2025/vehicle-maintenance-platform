using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class ReviewApis
{
    public static IEndpointRouteBuilder MapReviewApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/bookings")
            .MapBookingReviewRoutes()
            .WithTags("Review Api")
            .RequireRateLimiting("Fixed");

        builder.MapGroup("/api/v1/branches")
            .MapBranchReviewRoutes()
            .WithTags("Review Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapBookingReviewRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/review", SubmitReview)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateReviewRequest>())
            .WithName("SubmitReview")
            .RequireAuthorization()
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}/review", GetBookingReview)
            .WithName("GetBookingReview")
            .RequireAuthorization()
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageReviewResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static RouteGroupBuilder MapBranchReviewRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/{branchId:guid}/reviews", GetBranchReviews)
            .WithName("GetBranchReviews")
            .Produces<ApiResponse<List<GarageReviewResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> SubmitReview(
        [FromRoute] Guid id,
        [FromBody] CreateReviewRequest request,
        ICurrentUserService currentUserService,
        IReviewService reviewService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await reviewService.SubmitReviewAsync(userId, id, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBookingReview(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IReviewService reviewService,
        CancellationToken ct)
    {
        var viewerId = currentUserService.UserId;
        if (viewerId == Guid.Empty)
            return Results.Unauthorized();

        var result = await reviewService.GetByBookingAsync(id, viewerId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBranchReviews(
        [FromRoute] Guid branchId,
        [AsParameters] PaginationRequest pagination,
        IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetByBranchAsync(branchId, pagination, ct);
        return result.ToHttpResult();
    }
}
