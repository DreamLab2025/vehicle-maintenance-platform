namespace Verendar.Vehicle.Application.Dtos.Internal
{
    public class OdometerHistorySummaryDto
    {
        public int EntryCount { get; set; }
        public double? KmPerMonthAvg { get; set; }
        public double? KmPerMonthLast3Months { get; set; }
    }
}
