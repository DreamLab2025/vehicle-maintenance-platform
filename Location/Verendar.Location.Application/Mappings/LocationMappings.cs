namespace Verendar.Location.Application.Mappings;

public static class LocationMappings
{
    public static ProvinceResponse ToResponse(this Province entity)
    {
        return new ProvinceResponse
        {
            Code = entity.Code,
            Name = entity.Name,
            AdministrativeRegionId = entity.AdministrativeRegionId,
            AdministrativeRegionName = entity.AdministrativeRegion?.Name ?? string.Empty,
            AdministrativeUnitId = entity.AdministrativeUnitId,
            AdministrativeUnitName = entity.AdministrativeUnit?.Name ?? string.Empty
        };
    }

    public static WardResponse ToResponse(this Ward entity)
    {
        return new WardResponse
        {
            Code = entity.Code,
            Name = entity.Name,
            ProvinceCode = entity.ProvinceCode,
            ProvinceName = entity.Province?.Name ?? string.Empty,
            AdministrativeUnitId = entity.AdministrativeUnitId,
            AdministrativeUnitName = entity.AdministrativeUnit?.Name ?? string.Empty
        };
    }

    public static AdministrativeUnitResponse ToResponse(this AdministrativeUnit entity)
    {
        return new AdministrativeUnitResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Abbreviation = entity.Abbreviation
        };
    }

    public static AdministrativeRegionResponse ToResponse(this AdministrativeRegion entity)
    {
        return new AdministrativeRegionResponse
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
