namespace Verendar.Vehicle.Application.Dtos
{
    public class UserVehicleRequest
    {
        public Guid VehicleVariantId { get; set; }

        public string LicensePlate { get; set; } = null!;

        public string? VinNumber { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public int CurrentOdometer { get; set; }
    }

    public class UpdateOdometerRequest
    {
        public int CurrentOdometer { get; set; }
    }

    public class UserVehicleResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? LicensePlate { get; set; }
        public string? VinNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int CurrentOdometer { get; set; }
        public DateTime? LastOdometerUpdateAt { get; set; }
        public int? AverageKmPerDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserVehicleVariantResponse UserVehicleVariant { get; set; } = null!;
    }

    public class UserVehicleDetailResponse : UserVehicleResponse
    {
        public int TotalMaintenanceActivities { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public int DaysSincePurchase { get; set; }
        public int TotalKmDriven { get; set; }
        public List<VehiclePartTrackingSummary> PartTrackings { get; set; } = new();
    }

    public class VehiclePartTrackingSummary
    {
        public Guid Id { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; } = null!;
        public string PartCategoryCode { get; set; } = null!;
        public string? InstanceIdentifier { get; set; }
        public Guid? CurrentPartProductId { get; set; }
        public string? CurrentPartProductName { get; set; }
        public int? LastReplacementOdometer { get; set; }
        public DateOnly? LastReplacementDate { get; set; }
        public int? CustomKmInterval { get; set; }
        public int? CustomMonthsInterval { get; set; }
        public int? PredictedNextOdometer { get; set; }
        public DateOnly? PredictedNextDate { get; set; }
        public bool IsIgnored { get; set; }
        public string? UserConditionDescription { get; set; }
        public string? AiAnalysisResult { get; set; }
        public List<MaintenanceReminderSummary> Reminders { get; set; } = new();
    }

    public class MaintenanceReminderSummary
    {
        public Guid Id { get; set; }
        public string Level { get; set; } = null!;
        public int CurrentOdometer { get; set; }
        public int TargetOdometer { get; set; }
        public DateOnly? TargetDate { get; set; }
        public decimal PercentageRemaining { get; set; }
        public bool IsNotified { get; set; }
        public DateOnly? NotifiedDate { get; set; }
        public bool IsDismissed { get; set; }
        public DateOnly? DismissedDate { get; set; }
    }

    public class VehicleStreakResponse
    {
        public Guid VehicleId { get; set; }
        public int CurrentStreak { get; set; }
        public bool IsStreakActive { get; set; }
        public int DaysToNextUnlock { get; set; }
    }
}
