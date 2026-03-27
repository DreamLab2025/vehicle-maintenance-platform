using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(BookingId))]
public class BookingLineItem : BaseEntity
{
    public Guid BookingId { get; set; }

    /// <summary>Exactly one of ProductId / ServiceId / BundleId must be non-null.</summary>
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? BundleId { get; set; }

    /// <summary>Chỉ có nghĩa khi ProductId != null và product có InstallationServiceId.</summary>
    public bool IncludeInstallation { get; set; }

    /// <summary>Snapshot giá tại thời điểm đặt lịch.</summary>
    public Money BookedItemPrice { get; set; } = null!;

    public int SortOrder { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
    public GarageProduct? Product { get; set; }
    public GarageService? Service { get; set; }
    public GarageBundle? Bundle { get; set; }
}
