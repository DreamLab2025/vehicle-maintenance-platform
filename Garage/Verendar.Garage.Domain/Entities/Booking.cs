using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
[Index(nameof(UserId))]
public class Booking : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    public Guid GarageProductId { get; set; }

    public Guid UserId { get; set; }

    public Guid UserVehicleId { get; set; }

    public Guid? MechanicId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [MaxLength(1000)]
    public string? Note { get; set; }

    public Money BookedPrice { get; set; } = null!;

    public DateTime? CompletedAt { get; set; }

    public int? CurrentOdometer { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public Guid? PaymentId { get; set; }

    public GarageBranch GarageBranch { get; set; } = null!;
    public GarageProduct GarageProduct { get; set; } = null!;
    public Mechanic? Mechanic { get; set; }
    public List<BookingStatusHistory> StatusHistory { get; set; } = [];
    public GarageReview? Review { get; set; }
}
