namespace Verendar.Common.Stats;

public record ChartTimelineResponse(
    string GroupBy,
    DateOnly From,
    DateOnly To,
    List<ChartPoint> Points);

public record ChartPoint(string Period, decimal Value);
