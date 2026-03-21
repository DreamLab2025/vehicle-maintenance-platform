namespace Verendar.Location.Domain.Entities;

/// <summary>
/// Vietnamese provinces/cities per NQ202/2025/QH15 - 63 total
/// PK is string Code (not Guid) for better data integrity
/// No audit columns (reference data)
/// </summary>
public class Province
{
    [Key]
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int AdministrativeRegionId { get; set; }

    public AdministrativeRegion? AdministrativeRegion { get; set; }

    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
