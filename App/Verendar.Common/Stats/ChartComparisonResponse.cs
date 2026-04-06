namespace Verendar.Common.Stats;

public record ChartComparisonResponse(
    string GroupBy,
    DateOnly From,
    DateOnly To,
    List<string> Labels,
    List<ChartSeries> Series);

public record ChartSeries(string Name, List<decimal> Data);
