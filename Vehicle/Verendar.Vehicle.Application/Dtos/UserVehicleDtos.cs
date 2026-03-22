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

    public class UpdateOdometerResponse
    {
        public Guid UserVehicleId { get; set; }
        public int CurrentOdometer { get; set; }
        public DateTime? LastOdometerUpdateAt { get; set; }
    }

    public class ApplyTrackingConfigRequest
    {
        public string PartCategoryCode { get; set; } = string.Empty;
        public int? LastReplacementOdometer { get; set; }
        public DateOnly? LastReplacementDate { get; set; }
        public int? PredictedNextOdometer { get; set; }
        public DateOnly? PredictedNextDate { get; set; }
        public string? AiReasoning { get; set; }
        public double? ConfidenceScore { get; set; }
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
        public bool NeedsOnboarding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserVariantResponse UserVehicleVariant { get; set; } = null!;
    }

    public class VehicleTypeRefSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class VehicleBrandRefSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public VehicleTypeRefSummaryDto Type { get; set; } = null!;
    }

    public class VehicleModelRefSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public VehicleBrandRefSummaryDto Brand { get; set; } = null!;
    }

    public class UserVehicleVariantSummaryDto
    {
        public Guid Id { get; set; }
        public string Color { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public VehicleModelRefSummaryDto Model { get; set; } = null!;
    }

    public class UserVehicleSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? LicensePlate { get; set; }
        public string? VinNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int CurrentOdometer { get; set; }
        public DateTime? LastOdometerUpdateAt { get; set; }
        public int? AverageKmPerDay { get; set; }
        public bool NeedsOnboarding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserVehicleVariantSummaryDto Variant { get; set; } = null!;
    }


    public class UserVehicleDetailResponse : UserVehicleResponse
    {
        public int TotalMaintenanceActivities { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public int DaysSincePurchase { get; set; }
        public int TotalKmDriven { get; set; }
    }


    public class PartSummary
    {
        public Guid Id { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; } = null!;
        public string PartCategoryCode { get; set; } = null!;
        public string? IconUrl { get; set; }
        public bool IsDeclared { get; set; }
        public string? Description { get; set; }
    }

    public class TrackingCycleSummary
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public int StartOdometer { get; set; }
        public DateOnly StartDate { get; set; }
        public int? TargetOdometer { get; set; }
        public DateOnly? TargetDate { get; set; }
        public List<ReminderSummary> Reminders { get; set; } = new();
    }

    public class PartTrackingSummary
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
        public bool IsDeclared { get; set; }
        public TrackingCycleSummary? ActiveCycle { get; set; }
    }

    public class ReminderSummary
    {
        public Guid Id { get; set; }
        public string Level { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int CurrentOdometer { get; set; }
        public int TargetOdometer { get; set; }
        public int RemainingKm { get; set; }
        public DateOnly? TargetDate { get; set; }
        public decimal PercentageRemaining { get; set; }
        public bool IsNotified { get; set; }
        public DateOnly? NotifiedDate { get; set; }
        public bool IsDismissed { get; set; }
        public DateOnly? DismissedDate { get; set; }
    }

    public class StreakResponse
    {
        public Guid VehicleId { get; set; }
        public int CurrentStreak { get; set; }
        public bool IsStreakActive { get; set; }
        public int DaysToNextUnlock { get; set; }
    }

    public class IsAllowedToCreateVehicleResponse
    {
        public bool IsAllowed { get; set; }
        public string? Message { get; set; }
    }

    public class ReminderDetailDto
    {
        public Guid Id { get; set; }
        public Guid TrackingCycleId { get; set; }
        public string Level { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int CurrentOdometer { get; set; }
        public int TargetOdometer { get; set; }
        public int RemainingKm { get; set; }
        public DateOnly? TargetDate { get; set; }
        public decimal PercentageRemaining { get; set; }
        public bool IsNotified { get; set; }
        public DateOnly? NotifiedDate { get; set; }
        public bool IsDismissed { get; set; }
        public DateOnly? DismissedDate { get; set; }
        public CategoryInfoDto PartCategory { get; set; } = null!;
    }

    public class CategoryInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string? IdentificationSigns { get; set; }
        public string? ConsequencesIfNotHandled { get; set; }
    }

    public class OdometerHistoryQueryRequest : PaginationRequest
    {
        public Guid UserVehicleId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public override void Normalize()
        {
            base.Normalize();
        }
    }

    public class OdometerHistoryItemDto
    {
        public Guid Id { get; set; }
        public Guid UserVehicleId { get; set; }
        public int OdometerValue { get; set; }
        public DateOnly RecordedDate { get; set; }
        public int? KmOnRecordedDate { get; set; }
        public string Source { get; set; } = null!;
    }

    public class VehicleHealthScoreResponse
    {
        public Guid VehicleId { get; set; }
        public decimal? Score { get; set; }
        public int TrackedPartCount { get; set; }
        public List<PartHealthItem> Breakdown { get; set; } = [];
    }

    public class PartHealthItem
    {
        public Guid PartTrackingId { get; set; }
        public string PartCategoryCode { get; set; } = null!;
        public string PartCategoryName { get; set; } = null!;
        public string? IconUrl { get; set; }
        public int HealthScore { get; set; }
        public string Status { get; set; } = null!;
    }
}
