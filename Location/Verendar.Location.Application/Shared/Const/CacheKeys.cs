namespace Verendar.Location.Application.Shared.Const;

public static class CacheKeys
{
    private const string LocationPrefix = "location";

    private const string SchemaVersion = "v4";

    public const string ProvincesAll = $"{LocationPrefix}:provinces:{SchemaVersion}";
    public static string ProvinceByCode(string code) => $"{LocationPrefix}:provinces:{code}:{SchemaVersion}";
    public static string WardsOfProvince(string provinceCode) => $"{LocationPrefix}:provinces:{provinceCode}:wards:{SchemaVersion}";
    public static string WardByCode(string code) => $"{LocationPrefix}:wards:{code}:{SchemaVersion}";
    public const string AdministrativeUnitsAll = $"{LocationPrefix}:administrative-units";
    public const string AdministrativeRegionsAll = $"{LocationPrefix}:administrative-regions";
    public static string PlaceSearch(string queryHash) => $"{LocationPrefix}:search:{queryHash}";
    public static string PlaceDetail(string placeId) => $"{LocationPrefix}:place:{placeId}";
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(24);
    public static readonly TimeSpan PlaceSearchCacheDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan PlaceDetailCacheDuration = TimeSpan.FromHours(1);
}
