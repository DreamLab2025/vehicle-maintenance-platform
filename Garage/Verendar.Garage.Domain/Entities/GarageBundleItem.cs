namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBundleId))]
public class GarageBundleItem : BaseEntity
{
    public Guid GarageBundleId { get; set; }

    /// <summary>Set khi item là phụ tùng. Exactly one of ProductId / ServiceId must be non-null.</summary>
    public Guid? ProductId { get; set; }

    /// <summary>Set khi item là dịch vụ. Exactly one of ProductId / ServiceId must be non-null.</summary>
    public Guid? ServiceId { get; set; }

    /// <summary>Chỉ có nghĩa khi ProductId != null và product có InstallationServiceId.</summary>
    public bool IncludeInstallation { get; set; }

    public int SortOrder { get; set; }

    // Navigation
    public GarageBundle GarageBundle { get; set; } = null!;
    public GarageProduct? Product { get; set; }
    public GarageService? Service { get; set; }
}
