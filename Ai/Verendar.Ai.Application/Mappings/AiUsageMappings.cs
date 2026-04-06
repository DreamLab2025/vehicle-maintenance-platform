using Verendar.Ai.Domain.QueryResults;

namespace Verendar.Ai.Application.Mappings;

public static class AiUsageMappings
{
    public static AiUsageModelStatsResponse ToResponse(this AiUsageByModelSummary s) =>
        new()
        {
            Model = s.Model,
            RequestCount = s.RequestCount,
            TotalInputTokens = s.TotalInputTokens,
            TotalOutputTokens = s.TotalOutputTokens,
            TotalTokens = s.TotalTokens,
            TotalCost = s.TotalCost,
            FailedRequestCount = s.FailedRequestCount,
            FirstUsedAtUtc = s.FirstUsedAtUtc,
            LastUsedAtUtc = s.LastUsedAtUtc,
        };
}
