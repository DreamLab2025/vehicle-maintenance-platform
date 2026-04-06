using Verendar.Common.Stats;

namespace Verendar.Identity.Application.Services.Interfaces;

public interface IIdentityStatsService
{
    Task<ApiResponse<IdentityOverviewStatsResponse>> GetOverviewAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<ApiResponse<ChartTimelineResponse>> GetUserGrowthAsync(ChartQueryRequest request, CancellationToken ct = default);
}
