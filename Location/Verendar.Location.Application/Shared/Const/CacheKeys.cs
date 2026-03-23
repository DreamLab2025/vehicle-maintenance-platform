namespace Verendar.Location.Application.Shared.Const;

public static class CacheKeys
{
    private const string LocationPrefix = "location";

    // Provinces
    public const string ProvincesAll = $"{LocationPrefix}:provinces:all";
    public static string ProvinceByCode(string code) => $"{LocationPrefix}:provinces:{code}";
    public static string WardsOfProvince(string provinceCode) => $"{LocationPrefix}:provinces:{provinceCode}:wards";

    // Wards
    public static string WardByCode(string code) => $"{LocationPrefix}:wards:{code}";

    // Administrative Units & Regions
    public const string AdministrativeUnitsAll = $"{LocationPrefix}:administrative-units";
    public const string AdministrativeRegionsAll = $"{LocationPrefix}:administrative-regions";

    // Cache Duration
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(24);
}
