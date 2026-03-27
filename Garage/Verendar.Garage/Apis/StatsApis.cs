using Verendar.Common.Jwt;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class StatsApis
{
    public static IEndpointRouteBuilder MapStatsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages")
            .MapStatsRoutes()
            .WithTags("Stats Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapStatsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/{garageId:guid}/stats", GetGarageStats)
            .WithName("GetGarageStats")
            .WithOpenApi(op => { op.Summary = "Thống kê doanh thu và hoạt động toàn garage (chỉ chủ garage)"; return op; })
            .AddEndpointFilter(ValidationEndpointFilter.Validate<StatsRequest>())
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner)))
            .Produces<ApiResponse<GarageStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageStatsResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageStatsResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{garageId:guid}/branches/{branchId:guid}/stats", GetBranchStats)
            .WithName("GetBranchStats")
            .WithOpenApi(op => { op.Summary = "Thống kê doanh thu và hoạt động chi nhánh (chủ garage hoặc manager chi nhánh)"; return op; })
            .AddEndpointFilter(ValidationEndpointFilter.Validate<StatsRequest>())
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetGarageStats(
        Guid garageId,
        [AsParameters] StatsRequest request,
        IStatsService statsService,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        var result = await statsService.GetGarageStatsAsync(garageId, currentUser.UserId, request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetBranchStats(
        Guid garageId,
        Guid branchId,
        [AsParameters] StatsRequest request,
        IStatsService statsService,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        var result = await statsService.GetBranchStatsAsync(garageId, branchId, currentUser.UserId, request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
