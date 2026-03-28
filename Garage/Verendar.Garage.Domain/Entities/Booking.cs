using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
[Index(nameof(UserId))]
public class Booking : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    public Guid UserId { get; set; }

    public Guid UserVehicleId { get; set; }

    public Guid? MechanicId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [MaxLength(1000)]
    public string? Note { get; set; }

    /// <summary>Snapshot tổng giá tại thời điểm đặt lịch.</summary>
    public Money BookedTotalPrice { get; set; } = null!;

    public DateTime? CompletedAt { get; set; }

    public int? CurrentOdometer { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public Guid? PaymentId { get; set; }

    /// <summary>Snapshot thông tin xe tại thời điểm đặt lịch (JSON). Dùng khi hiển thị để tránh gọi Vehicle service.</summary>
    [MaxLength(2000)]
    public string? VehicleSnapshotJson { get; set; }

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
    public GarageMember? Mechanic { get; set; }
    public List<BookingLineItem> LineItems { get; set; } = [];
    public List<BookingStatusHistory> StatusHistory { get; set; } = [];
    public GarageReview? Review { get; set; }
}
