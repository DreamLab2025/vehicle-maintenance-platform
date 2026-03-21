namespace Verendar.Location.Domain.Entities;

/// <summary>
/// Administrative unit type labels used for UI dropdowns - e.g., "Tỉnh", "Thành phố", "Phường", "Xã"
/// Reference data, no audit columns
/// </summary>
public class AdministrativeUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Abbreviation { get; set; }
}
