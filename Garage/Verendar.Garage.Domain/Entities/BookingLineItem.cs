using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(BookingId))]
public class BookingLineItem : BaseEntity
{
    public Guid BookingId { get; set; }

    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? BundleId { get; set; }

    public bool IncludeInstallation { get; set; }

    public Money BookedItemPrice { get; set; } = null!;

    public int SortOrder { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
    public GarageProduct? Product { get; set; }
    public GarageService? Service { get; set; }
    public GarageBundle? Bundle { get; set; }
}
