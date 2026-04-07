namespace Verendar.Garage.Application.Dtos;

// ── Requests ──────────────────────────────────────────────────────────────────

public class GetBookingsRequest : PaginationRequest
{
    public bool? AssignedToMe { get; set; }
    public Guid? BranchId { get; set; }
    public BookingStatus? Status { get; set; }
}

public record CreateBookingRequest
{
    public Guid GarageBranchId { get; init; }
    public Guid UserVehicleId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Note { get; init; }
    public List<CreateBookingLineItemRequest> Items { get; init; } = [];
}

public record CreateBookingLineItemRequest
{
    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? BundleId { get; init; }
    public bool IncludeInstallation { get; init; }
    public int SortOrder { get; init; }
}

public record AssignBookingRequest
{
    public Guid GarageMemberId { get; init; }
}

public record UpdateBookingMechanicStatusRequest
{
    public BookingStatus Status { get; init; }
    public int? CurrentOdometer { get; init; }
}

// ── Nested response ───────────────────────────────────────────────────────────

public record BookingBranchSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public Guid GarageId { get; init; }
    public string GarageBusinessName { get; init; } = string.Empty;
}

public record BookingLineItemResponse
{
    public Guid Id { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? BundleId { get; init; }
    public bool IncludeInstallation { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public decimal BookedItemAmount { get; init; }
    public string BookedItemCurrency { get; init; } = "VND";
    public int SortOrder { get; init; }
    public BookingProductSummary? ProductDetails { get; init; }
    public BookingServiceSummary? ServiceDetails { get; init; }
    public BookingBundleSummary? BundleDetails { get; init; }
}

public record BookingProductSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public decimal MaterialPrice { get; init; }
    public string MaterialPriceCurrency { get; init; } = "VND";
    public int? EstimatedDurationMinutes { get; init; }
}

public record BookingServiceSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public decimal LaborPrice { get; init; }
    public string LaborPriceCurrency { get; init; } = "VND";
    public int? EstimatedDurationMinutes { get; init; }
}

public record BookingBundleSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercent { get; init; }
    public List<BookingBundleItemSummary> Items { get; init; } = [];
}

public record BookingBundleItemSummary
{
    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public bool IncludeInstallation { get; init; }
}

public record BookingStatusHistoryItemResponse
{
    public Guid Id { get; init; }
    public BookingStatus FromStatus { get; init; }
    public BookingStatus ToStatus { get; init; }
    public string? ChangedByName { get; init; }
    public string? Note { get; init; }
    public DateTime ChangedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record BookingCustomerSummary
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}

public record BookingVehicleSummary
{
    public Guid UserVehicleId { get; init; }
    public string? LicensePlate { get; init; }
    public string? Vin { get; init; }
    public int CurrentOdometer { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string VariantColor { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

// ── Booking responses ─────────────────────────────────────────────────────────

public record BookingResponse
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string VehicleBrand { get; init; } = string.Empty;
    public string VehicleModel { get; init; } = string.Empty;
    public Guid GarageBranchId { get; init; }
    public Guid? MechanicId { get; init; }
    public string? MechanicDisplayName { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Note { get; init; }
    public decimal BookedTotalAmount { get; init; }
    public string BookedCurrency { get; init; } = "VND";
    public DateTime? CompletedAt { get; init; }
    public int? CurrentOdometer { get; init; }
    public string? CancellationReason { get; init; }
    public Guid? PaymentId { get; init; }
    public BookingBranchSummary Branch { get; init; } = null!;
    public List<BookingLineItemResponse> LineItems { get; init; } = [];
    public IReadOnlyList<BookingStatusHistoryItemResponse> StatusHistory { get; init; } = [];
    public BookingCustomerSummary? Customer { get; init; }
    public BookingVehicleSummary? Vehicle { get; init; }
}

public record BookingListItemResponse
{
    public Guid Id { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public Guid GarageBranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public string ItemsSummary { get; init; } = string.Empty;
    public decimal BookedTotalAmount { get; init; }
    public string BookedCurrency { get; init; } = "VND";
}
