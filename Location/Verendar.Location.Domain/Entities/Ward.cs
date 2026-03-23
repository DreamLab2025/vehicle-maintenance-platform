namespace Verendar.Location.Domain.Entities;

public class Ward
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ProvinceCode { get; set; } = null!;

    public Province? Province { get; set; }

    public int AdministrativeUnitId { get; set; }

    public AdministrativeUnit? AdministrativeUnit { get; set; }
}
