using Verendar.Common.Stats;

namespace Verendar.Vehicle.Apis;

public static class StatsApis
{
    public static IEndpointRouteBuilder MapVehicleStatsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/vehicle")
            .MapStatsRoutes()
            .WithTags("Vehicle Stats Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapStatsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", GetOverviewStats)
            .WithName("GetVehicleOverviewStats")
            .WithOpenApi(op => { op.Summary = "Thống kê tổng quan Vehicle service (Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<VehicleOverviewStatsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/charts/maintenance-activity", GetMaintenanceActivityChart)
            .WithName("GetMaintenanceActivityChart")
            .WithOpenApi(op => { op.Summary = "Biểu đồ số maintenance records theo thời gian (Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ChartTimelineResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetOverviewStats(
        DateOnly? from,
        DateOnly? to,
        IVehicleStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetOverviewStatsAsync(from, to, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetMaintenanceActivityChart(
        [AsParameters] ChartQueryRequest request,
        IVehicleStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetMaintenanceActivityChartAsync(request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
