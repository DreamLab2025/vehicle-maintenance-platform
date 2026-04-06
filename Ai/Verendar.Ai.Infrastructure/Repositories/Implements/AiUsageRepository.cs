using Verendar.Ai.Domain.QueryResults;
using Verendar.Ai.Domain.Repositories.Interfaces;

namespace Verendar.Ai.Infrastructure.Repositories.Implements;

public class AiUsageRepository(AiDbContext context) : PostgresRepository<AiUsage>(context), IAiUsageRepository
{
    public async Task<(List<AiUsageByModelSummary> Items, int TotalCount)> GetAggregatedByModelPagedAsync(
        string? modelSearch,
        DateTime? fromUtc,
        DateTime? toUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AiUsage> q = AsQueryable();

        if (!string.IsNullOrEmpty(modelSearch))
        {
            var term = modelSearch.Trim().ToLowerInvariant();
            q = q.Where(u => u.Model.ToLower().Contains(term));
        }

        if (fromUtc.HasValue)
            q = q.Where(u => u.CreatedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            q = q.Where(u => u.CreatedAt <= toUtc.Value);

        var grouped = q.GroupBy(u => u.Model)
            .Select(g => new AiUsageByModelSummary
            {
                Model = g.Key,
                RequestCount = g.Count(),
                TotalInputTokens = g.Sum(x => (long)x.InputTokens),
                TotalOutputTokens = g.Sum(x => (long)x.OutputTokens),
                TotalTokens = g.Sum(x => (long)x.TotalTokens),
                TotalCost = g.Sum(x => x.TotalCost),
                FailedRequestCount = g.Count(x => x.ErrorMessage != null && x.ErrorMessage != ""),
                FirstUsedAtUtc = g.Min(x => x.CreatedAt),
                LastUsedAtUtc = g.Max(x => x.CreatedAt)
            });

        var totalCount = await grouped.CountAsync(cancellationToken);

        var items = await grouped
            .OrderByDescending(x => x.RequestCount)
            .ThenBy(x => x.Model)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
