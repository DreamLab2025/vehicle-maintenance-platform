using System.Security.Claims;
using Verendar.Common.Jwt;
using Verendar.Common.Stats;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class StatsApis
{
    public static IEndpointRouteBuilder MapStatsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garage")
            .MapStatsRoutes()
            .WithTags("Stats Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapStatsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", GetPlatformOverviewStats)
            .WithName("GetGarageOverviewStats")
            .WithOpenApi(op => { op.Summary = "Thống kê tổng quan toàn platform (Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<GarageOverviewStatsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{garageId:guid}/stats", GetGarageDetailStats)
            .WithName("GetGarageDetailStats")
            .WithOpenApi(op => { op.Summary = "Thống kê chi tiết một garage (chủ garage hoặc Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.Admin)))
            .Produces<ApiResponse<GarageDetailStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageDetailStatsResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageDetailStatsResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{garageId:guid}/branches/{branchId:guid}/stats", GetBranchStats)
            .WithName("GetBranchStats")
            .WithOpenApi(op => { op.Summary = "Thống kê chi tiết chi nhánh (chủ garage hoặc manager chi nhánh)"; return op; })
            .AddEndpointFilter(ValidationEndpointFilter.Validate<StatsRequest>())
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<BranchStatsResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        // ── Chart endpoints ─────────────────────────────────────────────────
        group.MapGet("/charts/booking-traffic", GetBookingTrafficChart)
            .WithName("GetBookingTrafficChart")
            .WithOpenApi(op => { op.Summary = "Biểu đồ lượng booking theo thời gian (Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ChartTimelineResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/charts/booking-outcomes", GetBookingOutcomesChart)
            .WithName("GetBookingOutcomesChart")
            .WithOpenApi(op => { op.Summary = "Biểu đồ so sánh Completed vs Cancelled theo thời gian (Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ChartComparisonResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{garageId:guid}/charts/revenue", GetGarageRevenueChart)
            .WithName("GetGarageRevenueChart")
            .WithOpenApi(op => { op.Summary = "Biểu đồ doanh thu garage theo thời gian (chủ garage hoặc Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.Admin)))
            .Produces<ApiResponse<RevenueChartResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RevenueChartResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<RevenueChartResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetPlatformOverviewStats(
        DateOnly? from,
        DateOnly? to,
        IStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetPlatformOverviewStatsAsync(from, to, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetGarageDetailStats(
        Guid garageId,
        DateOnly? from,
        DateOnly? to,
        IStatsService statsService,
        ICurrentUserService currentUser,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var isAdmin = principal.IsInRole(nameof(RoleType.Admin));
        var result = await statsService.GetGarageDetailStatsAsync(garageId, currentUser.UserId, isAdmin, from, to, ct);
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

    private static async Task<IResult> GetBookingTrafficChart(
        [AsParameters] ChartQueryRequest request,
        IStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetBookingTrafficChartAsync(request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetBookingOutcomesChart(
        [AsParameters] ChartQueryRequest request,
        IStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetBookingOutcomesChartAsync(request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetGarageRevenueChart(
        Guid garageId,
        [AsParameters] ChartQueryRequest request,
        IStatsService statsService,
        ICurrentUserService currentUser,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var isAdmin = principal.IsInRole(nameof(RoleType.Admin));
        var result = await statsService.GetGarageRevenueChartAsync(garageId, currentUser.UserId, isAdmin, request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
