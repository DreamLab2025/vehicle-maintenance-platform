namespace Verendar.Ai.Domain.QueryResults;

/// <summary>Aggregated AI usage for a single model id (grouped row).</summary>
public sealed class AiUsageByModelSummary
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
