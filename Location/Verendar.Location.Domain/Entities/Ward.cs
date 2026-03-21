namespace Verendar.Location.Domain.Entities;

/// <summary>
/// Wards/communes in Vietnamese provinces - ~11,000 total per NQ202/2025/QH15
/// PK is string Code (not Guid) for better data integrity
/// FK to Province by code, FK to AdministrativeUnit by id
/// No audit columns (reference data)
/// </summary>
public class Ward
{
    [Key]
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ProvinceCode { get; set; } = null!;

    public Province? Province { get; set; }

    public int AdministrativeUnitId { get; set; }

    public AdministrativeUnit? AdministrativeUnit { get; set; }
}
