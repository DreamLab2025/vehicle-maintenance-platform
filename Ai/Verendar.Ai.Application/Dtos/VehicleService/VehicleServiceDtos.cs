namespace Verendar.Ai.Application.Dtos.VehicleService
{
    public class VehicleServiceUserVehicleResponse
    {
        public int CurrentOdometer { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public VehicleServiceVariantResponse UserVehicleVariant { get; set; } = null!;
    }

    public class VehicleServiceVariantResponse
    {
        public Guid VehicleModelId { get; set; }
        public VehicleServiceModelResponse Model { get; set; } = null!;
    }

    public class VehicleServiceModelResponse
    {
        public string Name { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
    }

    public class VehicleServiceDefaultScheduleResponse
    {
        public int InitialKm { get; set; }
        public int KmInterval { get; set; }
        public int MonthsInterval { get; set; }
        public bool RequiresOdometerTracking { get; set; }
        public bool RequiresTimeTracking { get; set; }
    }

    public class VehicleServiceOdometerSummaryResponse
    {
        public int EntryCount { get; set; }
        public double? KmPerMonthAvg { get; set; }
        public double? KmPerMonthLast3Months { get; set; }
    }

    public class VehicleServiceBaselinePartItem
    {
        public Guid PartTrackingId { get; set; }
        public string PartCategorySlug { get; set; } = string.Empty;
        public Guid VehicleModelId { get; set; }
    }

    public class VehicleServiceApplyTrackingRequest
    {
        public string PartCategorySlug { get; set; } = string.Empty;
        public int? LastReplacementOdometer { get; set; }
        public DateOnly? LastReplacementDate { get; set; }
        public int? PredictedNextOdometer { get; set; }
        public DateOnly? PredictedNextDate { get; set; }
        public string? AiReasoning { get; set; }
        public bool IsBaseline { get; set; } = false;
    }
}
