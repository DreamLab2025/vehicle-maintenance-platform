using Verendar.Common.Stats;
using Verendar.Identity.Application.Services.Interfaces;

namespace Verendar.Identity.Apis;

public static class IdentityStatsApis
{
    public static IEndpointRouteBuilder MapIdentityStatsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/identity")
            .MapIdentityStatsRoutes()
            .WithTags("Identity Stats Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapIdentityStatsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", GetOverviewStats)
            .WithName("GetIdentityOverviewStats")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<IdentityOverviewStatsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/charts/user-growth", GetUserGrowthChart)
            .WithName("GetUserGrowthChart")
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ChartTimelineResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ChartTimelineResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetOverviewStats(
        DateOnly? from,
        DateOnly? to,
        IIdentityStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetOverviewAsync(from, to, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserGrowthChart(
        [AsParameters] ChartQueryRequest request,
        IIdentityStatsService statsService,
        CancellationToken ct)
    {
        var result = await statsService.GetUserGrowthAsync(request, ct);
        return result.ToHttpResult();
    }
}
