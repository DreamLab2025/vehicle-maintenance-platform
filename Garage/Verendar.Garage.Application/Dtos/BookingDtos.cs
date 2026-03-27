namespace Verendar.Garage.Application.Dtos;

public record CreateBookingRequest
{
    public Guid GarageBranchId { get; init; }
    public Guid GarageProductId { get; init; }
    public Guid UserVehicleId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Note { get; init; }
}

public record BookingResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid UserVehicleId { get; init; }
    public Guid GarageBranchId { get; init; }
    public Guid GarageProductId { get; init; }
    public Guid? MechanicId { get; init; }
    public string? MechanicDisplayName { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Note { get; init; }
    public decimal BookedAmount { get; init; }
    public string BookedCurrency { get; init; } = "VND";
    public DateTime? CompletedAt { get; init; }
    public int? CurrentOdometer { get; init; }
    public string? CancellationReason { get; init; }
    public Guid? PaymentId { get; init; }
    public BookingBranchSummary Branch { get; init; } = null!;
    public BookingProductSummary Product { get; init; } = null!;
    public IReadOnlyList<BookingStatusHistoryItemResponse> StatusHistory { get; init; } = [];
    public BookingCustomerSummary? Customer { get; init; }
    public BookingVehicleSummary? Vehicle { get; init; }
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

public record BookingBranchSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public Guid GarageId { get; init; }
    public string GarageBusinessName { get; init; } = string.Empty;
}

public record BookingProductSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ProductType Type { get; init; }
    public Guid? PartCategoryId { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
}

public record BookingStatusHistoryItemResponse
{
    public Guid Id { get; init; }
    public BookingStatus FromStatus { get; init; }
    public BookingStatus ToStatus { get; init; }
    public Guid ChangedByUserId { get; init; }
    public string? Note { get; init; }
    public DateTime ChangedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record BookingListItemResponse
{
    public Guid Id { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public Guid GarageBranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal BookedAmount { get; init; }
    public string BookedCurrency { get; init; } = "VND";
}
