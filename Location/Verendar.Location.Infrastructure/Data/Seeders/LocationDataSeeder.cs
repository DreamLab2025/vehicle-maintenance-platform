namespace Verendar.Location.Infrastructure.Data.Seeders;

public static class LocationDataSeeder
{
    public static IReadOnlyDictionary<string, int> ProvinceAdministrativeRegionIds { get; } =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["01"] = 3,
            ["04"] = 1,
            ["08"] = 1,
            ["11"] = 2,
            ["12"] = 2,
            ["14"] = 2,
            ["15"] = 2,
            ["19"] = 1,
            ["20"] = 1,
            ["22"] = 1,
            ["24"] = 3,
            ["25"] = 3,
            ["31"] = 3,
            ["33"] = 3,
            ["37"] = 3,
            ["38"] = 4,
            ["40"] = 4,
            ["42"] = 4,
            ["44"] = 4,
            ["46"] = 4,
            ["48"] = 5,
            ["51"] = 5,
            ["52"] = 6,
            ["56"] = 5,
            ["66"] = 6,
            ["68"] = 6,
            ["75"] = 7,
            ["79"] = 7,
            ["80"] = 7,
            ["82"] = 8,
            ["86"] = 8,
            ["91"] = 8,
            ["92"] = 8,
            ["96"] = 8,
        };
}
