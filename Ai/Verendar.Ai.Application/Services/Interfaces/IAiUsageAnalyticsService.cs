using Verendar.Ai.Application.Dtos.Ai;

namespace Verendar.Ai.Application.Services.Interfaces;

public interface IAiUsageAnalyticsService
{
    Task<ApiResponse<List<AiUsageModelStatsResponse>>> GetUsageByModelPagedAsync(
        AiUsageStatsQueryRequest query,
        CancellationToken cancellationToken = default);
}
