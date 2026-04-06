namespace Verendar.Common.Stats;

public record ChartQueryRequest
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string GroupBy { get; init; } = "month";

    public string? Validate()
    {
        var gb = (GroupBy ?? "month").ToLowerInvariant();
        if (gb != "day" && gb != "month")
            return "groupBy must be 'day' or 'month'";

        if (From.HasValue && To.HasValue && From > To)
            return "from must be ≤ to";

        if (From.HasValue && To.HasValue)
        {
            var diff = To.Value.DayNumber - From.Value.DayNumber;
            if (diff > 365 * 2)
                return "Date range must not exceed 2 years";
        }

        return null;
    }

    public (DateTime From, DateTime To, string GroupBy) Normalize()
    {
        var today = DateTime.UtcNow.Date;
        var toDate = To ?? DateOnly.FromDateTime(today);
        var fromDate = From ?? toDate.AddMonths(-12);
        var gb = (GroupBy ?? "month").ToLowerInvariant();

        return (
            fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
            gb
        );
    }
}
