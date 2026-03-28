namespace Verendar.Vehicle.Application.Dtos
{
    public class RecordItemInput
    {
        public string PartCategorySlug { get; set; } = string.Empty;
        /// <summary>Tham chiếu sản phẩm garage (cross-service, không FK trong Vehicle DB).</summary>
        public Guid? GarageProductId { get; set; }
        public string? CustomPartName { get; set; }
        public int? CustomKmInterval { get; set; }
        public int? CustomMonthsInterval { get; set; }
        public string? InstanceIdentifier { get; set; }
        public decimal? Price { get; set; }
        public string? ItemNotes { get; set; }
        public bool UpdatesTracking { get; set; } = true;
    }

    public class CreateRecordRequest
    {
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public string? GarageName { get; set; }
        public decimal? TotalCost { get; set; }
        public string? Notes { get; set; }
        public string? InvoiceImageUrl { get; set; }
        public List<RecordItemInput> Items { get; set; } = [];
    }

    public class RecordItemResult
    {
        public Guid MaintenanceRecordItemId { get; set; }
        public string PartCategorySlug { get; set; } = null!;
        public PartTrackingSummary Tracking { get; set; } = null!;
    }

    public class CreateRecordResponse
    {
        public Guid MaintenanceRecordId { get; set; }
        public List<RecordItemResult> Items { get; set; } = [];
    }


    public class RecordItemDto
    {
        public Guid Id { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategorySlug { get; set; } = null!;
        public Guid? GarageProductId { get; set; }
        public string? CustomPartName { get; set; }
        public string? InstanceIdentifier { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
        public bool UpdatesTracking { get; set; }
    }

    public class RecordSummaryDto
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

    public class RecordDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserVehicleId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public int OdometerAtService { get; set; }
        public string? GarageName { get; set; }
        public decimal TotalCost { get; set; }
        public string? Notes { get; set; }
        public string? InvoiceImageUrl { get; set; }
        public List<RecordItemDto> Items { get; set; } = [];
    }
}
