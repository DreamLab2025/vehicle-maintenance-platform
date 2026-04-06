namespace Verendar.Ai.Application.Dtos.Ai;

public class AiUsageStatsQueryRequest : PaginationRequest
{
    public string? ModelSearch { get; set; }

    public DateTime? FromUtc { get; set; }

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
