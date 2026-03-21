namespace Verendar.Vehicle.Application.Dtos
{
    public class DefaultScheduleResponse
    {
        public int InitialKm { get; set; }
        public int KmInterval { get; set; }
        public int MonthsInterval { get; set; }
        public bool RequiresOdometerTracking { get; set; }
        public bool RequiresTimeTracking { get; set; }
    }
}
