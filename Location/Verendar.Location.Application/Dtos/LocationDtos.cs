namespace Verendar.Location.Application.Dtos;

public class ProvinceResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int AdministrativeRegionId { get; set; }
    public string AdministrativeRegionName { get; set; } = string.Empty;
    public int AdministrativeUnitId { get; set; }
    public string AdministrativeUnitName { get; set; } = string.Empty;
}

public class WardResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ProvinceCode { get; set; } = null!;
    public string ProvinceName { get; set; } = string.Empty;
    public int AdministrativeUnitId { get; set; }
    public string AdministrativeUnitName { get; set; } = string.Empty;
}

public class AdministrativeUnitResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Abbreviation { get; set; }
}

public class AdministrativeRegionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
