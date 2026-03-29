namespace Verendar.Location.Application.Shared.Const;

public static class CacheKeys
{
    private const string LocationPrefix = "location";

    private const string SchemaVersion = "v2";

    public const string ProvincesAll = $"{LocationPrefix}:provinces:{SchemaVersion}";
    public static string ProvinceByCode(string code) => $"{LocationPrefix}:provinces:{code}:{SchemaVersion}";
    public static string WardsOfProvince(string provinceCode) => $"{LocationPrefix}:provinces:{provinceCode}:wards:{SchemaVersion}";
    public static string WardByCode(string code) => $"{LocationPrefix}:wards:{code}:{SchemaVersion}";
    public const string AdministrativeUnitsAll = $"{LocationPrefix}:administrative-units";
    public const string AdministrativeRegionsAll = $"{LocationPrefix}:administrative-regions";
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(24);
}
