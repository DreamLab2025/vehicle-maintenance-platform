namespace Verendar.Location.Domain.Entities;

public class Province
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int AdministrativeRegionId { get; set; }

    public AdministrativeRegion? AdministrativeRegion { get; set; }

    public int AdministrativeUnitId { get; set; }

    public AdministrativeUnit? AdministrativeUnit { get; set; }

    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
