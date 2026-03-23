namespace Verendar.Location.Domain.Entities;

/// <summary>
/// Administrative region in Vietnam (vùng miền) - e.g., "Vùng Đông Bắc", "Vùng Tây Nam"
/// Reference data, no audit columns
/// </summary>
public class AdministrativeRegion
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
