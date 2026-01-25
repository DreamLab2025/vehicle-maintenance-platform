namespace Verendar.Vehicle.Application.Dtos
{
    public class DefaultMaintenanceScheduleResponse
    {
        public Guid Id { get; set; }

        public Guid PartCategoryId { get; set; }

        public string PartCategoryCode { get; set; } = string.Empty;

        public string PartCategoryName { get; set; } = string.Empty;

        public string? PartCategoryDescription { get; set; }

        public string? IconUrl { get; set; }

        public int InitialKm { get; set; }

        public int KmInterval { get; set; }
    
        public int MonthsInterval { get; set; }

        public bool RequiresOdometerTracking { get; set; }

        public bool RequiresTimeTracking { get; set; }

        public int DisplayOrder { get; set; }
    }
}
