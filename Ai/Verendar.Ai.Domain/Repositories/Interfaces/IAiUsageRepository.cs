using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.QueryResults;

namespace Verendar.Ai.Domain.Repositories.Interfaces;

public interface IAiUsageRepository : IGenericRepository<AiUsage>
{
    Task<(List<AiUsageByModelSummary> Items, int TotalCount)> GetAggregatedByModelPagedAsync(
        string? modelSearch,
        DateTime? fromUtc,
        DateTime? toUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
