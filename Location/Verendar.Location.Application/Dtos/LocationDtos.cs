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

public class ProvinceBoundaryResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? BoundaryUrl { get; set; }
}

public class WardBoundaryResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? BoundaryUrl { get; set; }

    /// <summary>
    /// File GeoJSON tại <see cref="BoundaryUrl"/> là một FeatureCollection <strong>shard</strong> dùng chung cho nhiều phường/xã (cùng tỉnh),
    /// không phải một polygon đơn lẻ. Khi vẽ bản đồ, chỉ giữ feature có
    /// <c>properties[BoundaryShardMatchProperty]</c> (chuỗi) bằng <see cref="BoundaryShardMatchValue"/>.
    /// </summary>
    public string BoundaryShardMatchProperty { get; init; } = "ma_xa";

    /// <summary>
    /// Mã phường/xã khớp với GeoJSON (5 ký tự, số 0 đầu). Đồng bộ với script <c>build-ward-boundary-urls.js</c>.
    /// </summary>
    public string BoundaryShardMatchValue { get; init; } = null!;
}
