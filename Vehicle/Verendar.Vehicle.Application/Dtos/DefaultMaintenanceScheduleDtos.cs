namespace Verendar.Vehicle.Application.Dtos
{
    /// <summary>
    /// Response DTO for default maintenance schedule
    /// Used by frontend to display manufacturer's recommended maintenance intervals
    /// and send to AI for analysis
    /// </summary>
    public class DefaultMaintenanceScheduleResponse
    {
        public Guid Id { get; set; }

        public Guid PartCategoryId { get; set; }

        /// <summary>
        /// Part category code (e.g., "engine_oil", "oil_filter", "brake_pad")
        /// Used by AI to identify parts
        /// </summary>
        public string PartCategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// Part category name (e.g., "Dầu động cơ", "Lọc dầu")
        /// </summary>
        public string PartCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Part category description
        /// </summary>
        public string? PartCategoryDescription { get; set; }

        /// <summary>
        /// Icon URL for the part category
        /// </summary>
        public string? IconUrl { get; set; }

        /// <summary>
        /// Initial service at this odometer (km)
        /// e.g., First oil change at 1000km
        /// </summary>
        public int InitialKm { get; set; }

        /// <summary>
        /// Service interval in kilometers
        /// e.g., Every 5000km after initial service
        /// </summary>
        public int KmInterval { get; set; }

        /// <summary>
        /// Service interval in months
        /// e.g., Every 6 months
        /// </summary>
        public int MonthsInterval { get; set; }

        /// <summary>
        /// Whether this part requires odometer-based tracking
        /// </summary>
        public bool RequiresOdometerTracking { get; set; }

        /// <summary>
        /// Whether this part requires time-based tracking
        /// </summary>
        public bool RequiresTimeTracking { get; set; }

        /// <summary>
        /// Display order for frontend sorting
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
