namespace Verendar.Vehicle.Application.Dtos
{
    public class MaintenanceRecordItemInput
    {
        public string PartCategoryCode { get; set; } = string.Empty;
        public Guid? PartProductId { get; set; }
        public string? CustomPartName { get; set; }
        public int? CustomKmInterval { get; set; }
        public int? CustomMonthsInterval { get; set; }
        public string? InstanceIdentifier { get; set; }
        public decimal? Price { get; set; }
        public string? ItemNotes { get; set; }
        public bool UpdatesTracking { get; set; } = true;
    }

    public class CreateMaintenanceRecordRequest
    {
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public string? GarageName { get; set; }
        public decimal? TotalCost { get; set; }
        public string? Notes { get; set; }
        public string? InvoiceImageUrl { get; set; }
        public List<MaintenanceRecordItemInput> Items { get; set; } = [];
    }

    public class CreateMaintenanceRecordItemResult
    {
        public Guid MaintenanceRecordItemId { get; set; }
        public string PartCategoryCode { get; set; } = null!;
        public VehiclePartTrackingSummary Tracking { get; set; } = null!;
    }

    public class CreateMaintenanceRecordResponse
    {
        public Guid MaintenanceRecordId { get; set; }
        public List<CreateMaintenanceRecordItemResult> Items { get; set; } = [];
    }


    public class MaintenanceRecordItemDto
    {
        public Guid Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartCategoryCode { get; set; } = null!;
        public Guid? PartProductId { get; set; }
        public string? InstanceIdentifier { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }

    public class MaintenanceRecordSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public string? GarageName { get; set; }
        public decimal TotalCost { get; set; }
        public string? Notes { get; set; }
        public string? InvoiceImageUrl { get; set; }
        public int ItemCount { get; set; }
    }

    public class MaintenanceRecordDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public string? GarageName { get; set; }
        public decimal TotalCost { get; set; }
        public string? Notes { get; set; }
        public string? InvoiceImageUrl { get; set; }
        public List<MaintenanceRecordItemDto> Items { get; set; } = [];
    }
}
