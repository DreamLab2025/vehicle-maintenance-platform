namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
public class GarageReview : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}
