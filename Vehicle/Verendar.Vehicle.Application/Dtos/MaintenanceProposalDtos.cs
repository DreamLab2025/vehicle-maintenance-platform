using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Dtos
{
    public class MaintenanceProposalItemResponse
    {
        public Guid Id { get; set; }
        public Guid? PartCategoryId { get; set; }
        public string? PartCategoryName { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public bool UpdatesTracking { get; set; }
        public int? RecommendedKmInterval { get; set; }
        public int? RecommendedMonthsInterval { get; set; }
        public decimal Price { get; set; }
    }

    public class MaintenanceProposalResponse
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserVehicleId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateOnly ServiceDate { get; set; }
        public int? OdometerAtService { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public ProposalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MaintenanceProposalItemResponse> Items { get; set; } = [];
    }

    public class UpdateProposalItemRequest
    {
        public Guid Id { get; set; }
        public bool UpdatesTracking { get; set; }
    }

    public class UpdateProposalRequest
    {
        public int? OdometerAtService { get; set; }
        public string? Notes { get; set; }
        public List<UpdateProposalItemRequest>? Items { get; set; }
    }

    public class ApplyProposalResult
    {
        public Guid MaintenanceRecordId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public List<string> TrackingUpdated { get; set; } = [];
    }
}
