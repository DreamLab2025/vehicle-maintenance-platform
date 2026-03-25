using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Dtos.Ai;

public class AiUsageStatsQueryRequest : PaginationRequest
{
    /// <summary>Optional substring match on model id (case-insensitive).</summary>
    public string? ModelSearch { get; set; }

    /// <summary>Inclusive lower bound on usage CreatedAt (UTC).</summary>
    public DateTime? FromUtc { get; set; }

    /// <summary>Inclusive upper bound on usage CreatedAt (UTC).</summary>
    public DateTime? ToUtc { get; set; }

    public override void Normalize()
    {
        base.Normalize();
        if (string.IsNullOrWhiteSpace(ModelSearch))
            ModelSearch = null;
        else
            ModelSearch = ModelSearch.Trim();
    }
}

public class AiUsageModelStatsResponse
{
    public string Model { get; set; } = null!;
    public int RequestCount { get; set; }
    public long TotalInputTokens { get; set; }
    public long TotalOutputTokens { get; set; }
    public long TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public int FailedRequestCount { get; set; }
    public DateTime FirstUsedAtUtc { get; set; }
    public DateTime LastUsedAtUtc { get; set; }
}
