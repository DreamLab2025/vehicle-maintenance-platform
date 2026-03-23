namespace Verendar.Garage.Domain.Entities;

[Index(nameof(BookingId))]
public class BookingStatusHistory : BaseEntity
{
    public Guid BookingId { get; set; }

    public BookingStatus FromStatus { get; set; }

    public BookingStatus ToStatus { get; set; }

    public Guid ChangedByUserId { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    public DateTime ChangedAt { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
}
