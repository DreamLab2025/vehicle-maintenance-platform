using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Mappings;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Repositories.Interfaces;

namespace Verendar.Ai.Application.Services.Implements;

public class AiUsageAnalyticsService(IUnitOfWork unitOfWork) : IAiUsageAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<AiUsageModelStatsResponse>>> GetUsageByModelPagedAsync(
        AiUsageStatsQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        query.Normalize();

        var (items, total) = await _unitOfWork.AiUsages.GetAggregatedByModelPagedAsync(
            query.ModelSearch,
            query.FromUtc,
            query.ToUtc,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        return ApiResponse<List<AiUsageModelStatsResponse>>.SuccessPagedResponse(
            items.Select(i => i.ToResponse()).ToList(),
            total,
            query.PageNumber,
            query.PageSize,
            "Lấy thống kê usage theo model thành công");
    }
}
