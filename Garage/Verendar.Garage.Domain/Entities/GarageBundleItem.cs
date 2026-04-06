namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBundleId))]
public class GarageBundleItem : BaseEntity
{
    public Guid GarageBundleId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? ServiceId { get; set; }
    
    public bool IncludeInstallation { get; set; }

    public int SortOrder { get; set; }

    // Navigation
    public GarageBundle GarageBundle { get; set; } = null!;
    public GarageProduct? Product { get; set; }
    public GarageService? Service { get; set; }
}
