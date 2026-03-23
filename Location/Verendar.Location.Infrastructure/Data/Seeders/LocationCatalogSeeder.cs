namespace Verendar.Location.Infrastructure.Data.Seeders;

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

public static partial class LocationCatalogSeeder
{
    private const string SeedSqlResourceName = "Verendar.Location.Infrastructure.Data.Seeders.seed_data.sql";
    private const int BatchSize = 500;

    public static async Task SeedAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken = default)
    {
        var sql = await ReadEmbeddedSqlAsync();
        if (string.IsNullOrEmpty(sql))
        {
            logger?.LogCritical("Embedded seed_data.sql missing or empty.");
            throw new InvalidOperationException(
                "Location seed requires embedded resource Data/Seeders/seed_data.sql.");
        }

        await SeedAdministrativeRegionsAsync(db, sql, logger, cancellationToken);
        await SeedAdministrativeUnitsAsync(db, sql, logger, cancellationToken);
        await SeedProvincesAsync(db, sql, logger, cancellationToken);
        await SeedWardsFromSqlAsync(db, sql, logger, cancellationToken);
    }

    private static async Task SeedAdministrativeRegionsAsync(
        LocationDbContext db,
        string sql,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.AdministrativeRegions.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Administrative regions already seeded, skipping");
            return;
        }

        var regions = ParseAdministrativeRegionsFromSql(sql);
        if (regions.Count == 0)
        {
            logger?.LogCritical("Parsed 0 administrative_regions from seed_data.sql.");
            throw new InvalidOperationException("Location seed: no administrative_regions parsed from seed_data.sql.");
        }

        db.AdministrativeRegions.AddRange(regions);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} administrative regions from SQL resource", regions.Count);
    }

    private static async Task SeedAdministrativeUnitsAsync(
        LocationDbContext db,
        string sql,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.AdministrativeUnits.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Administrative units already seeded, skipping");
            return;
        }

        var units = ParseAdministrativeUnitsFromSql(sql);
        if (units.Count == 0)
        {
            logger?.LogCritical("Parsed 0 administrative_units from seed_data.sql.");
            throw new InvalidOperationException("Location seed: no administrative_units parsed from seed_data.sql.");
        }

        db.AdministrativeUnits.AddRange(units);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} administrative units from SQL resource", units.Count);
    }

    private static async Task SeedProvincesAsync(
        LocationDbContext db,
        string fullSql,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.Provinces.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Provinces already seeded, skipping");
            return;
        }

        var provinces = ParseProvincesFromSql(fullSql);
        if (provinces.Count == 0)
        {
            logger?.LogCritical("Parsed 0 provinces from seed_data.sql.");
            throw new InvalidOperationException("Location seed: no provinces parsed from seed_data.sql.");
        }

        db.Provinces.AddRange(provinces);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} provinces from SQL resource", provinces.Count);
    }

    private static async Task SeedWardsFromSqlAsync(
        LocationDbContext db,
        string sql,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.Wards.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Wards already seeded, skipping");
            return;
        }

        var wards = ParseWardsFromSql(sql);
        if (wards.Count == 0)
        {
            logger?.LogCritical("Parsed 0 wards from seed_data.sql; check INSERT format.");
            throw new InvalidOperationException("Location seed: no ward rows parsed from seed_data.sql.");
        }

        logger?.LogInformation("Parsed {Count} wards from SQL resource", wards.Count);

        for (var i = 0; i < wards.Count; i += BatchSize)
        {
            var batch = wards.Skip(i).Take(BatchSize).ToList();
            db.Wards.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
        }

        logger?.LogInformation("Seeded {Count} wards from SQL resource", wards.Count);
    }

    private static async Task<string?> ReadEmbeddedSqlAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream(SeedSqlResourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private const string RegionsSectionStart = "-- DATA for administrative_regions --";
    private const string RegionsSectionEnd = "-- DATA for administrative_units --";
    private const string UnitsSectionStart = "-- DATA for administrative_units --";
    private const string UnitsSectionEnd = "-- DATA for provinces --";
    private const string WardsSectionStart = "-- DATA for wards --";

    private static List<AdministrativeRegion> ParseAdministrativeRegionsFromSql(string fullSql)
    {
        var sql = ExtractSqlSection(fullSql, RegionsSectionStart, RegionsSectionEnd);
        var list = new List<AdministrativeRegion>();
        var regex = AdministrativeRegionInsertRegex();

        foreach (Match match in regex.Matches(sql))
        {
            list.Add(new AdministrativeRegion
            {
                Id = int.Parse(match.Groups[1].Value),
                Name = UnescapeSqlString(match.Groups[2].Value)
            });
        }

        return list.OrderBy(r => r.Id).ToList();
    }

    private static List<AdministrativeUnit> ParseAdministrativeUnitsFromSql(string fullSql)
    {
        var sql = ExtractSqlSection(fullSql, UnitsSectionStart, UnitsSectionEnd);
        var list = new List<AdministrativeUnit>();
        var regex = AdministrativeUnitInsertRegex();

        foreach (Match match in regex.Matches(sql))
        {
            list.Add(new AdministrativeUnit
            {
                Id = int.Parse(match.Groups[1].Value),
                Name = UnescapeSqlString(match.Groups[2].Value),
                Abbreviation = UnescapeSqlString(match.Groups[3].Value)
            });
        }

        return list.OrderBy(u => u.Id).ToList();
    }

    private static string UnescapeSqlString(string s) => s.Replace("''", "'", StringComparison.Ordinal);

    private const string ProvincesSectionStart = "-- DATA for provinces --";
    private const string ProvincesSectionEnd = "-- DATA for wards --";

    private static List<Province> ParseProvincesFromSql(string fullSql)
    {
        var section = ExtractSqlSection(fullSql, ProvincesSectionStart, ProvincesSectionEnd);
        if (string.IsNullOrEmpty(section))
        {
            return [];
        }

        var map = LocationDataSeeder.ProvinceAdministrativeRegionIds;
        var list = new List<Province>();
        var regex = ProvinceRowRegex();

        foreach (Match match in regex.Matches(section))
        {
            var code = match.Groups[1].Value;
            var name = UnescapeSqlString(match.Groups[2].Value);
            var administrativeUnitId = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            if (!map.TryGetValue(code, out var regionId))
            {
                throw new InvalidOperationException(
                    $"Location seed: province code '{code}' has no AdministrativeRegionId mapping; update {nameof(LocationDataSeeder)}.");
            }

            list.Add(new Province
            {
                Code = code,
                Name = name,
                AdministrativeRegionId = regionId,
                AdministrativeUnitId = administrativeUnitId
            });
        }

        return list.OrderBy(p => p.Code, StringComparer.Ordinal).ToList();
    }

    private static string ExtractSqlSection(string sql, string startMarker, string? endMarker)
    {
        var start = sql.IndexOf(startMarker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += startMarker.Length;
        if (string.IsNullOrEmpty(endMarker))
        {
            return sql[start..];
        }

        var end = sql.IndexOf(endMarker, start, StringComparison.Ordinal);
        var sliceEnd = end < 0 ? sql.Length : end;
        return sql[start..sliceEnd];
    }

    private static List<Ward> ParseWardsFromSql(string fullSql)
    {
        var sql = ExtractSqlSection(fullSql, WardsSectionStart, null);
        var wards = new List<Ward>();
        var regex = WardRowRegex();

        foreach (Match match in regex.Matches(sql))
        {
            var code = match.Groups[1].Value;
            var name = UnescapeSqlString(match.Groups[2].Value);
            var provinceCode = match.Groups[3].Value;
            var unitId = int.Parse(match.Groups[4].Value);

            wards.Add(new Ward
            {
                Code = code,
                Name = name,
                ProvinceCode = provinceCode,
                AdministrativeUnitId = unitId
            });
        }

        return wards;
    }

    [GeneratedRegex(
        @"INSERT INTO administrative_regions\(id,name,name_en,code_name,code_name_en\) VALUES\((\d+),'((?:[^']|'')+)'")]
    private static partial Regex AdministrativeRegionInsertRegex();

    [GeneratedRegex(
        @"INSERT INTO administrative_units\(id,full_name,full_name_en,short_name,short_name_en,code_name,code_name_en\) VALUES\((\d+),'((?:[^']|'')+)','(?:[^']|'')+','((?:[^']|'')+)'")]
    private static partial Regex AdministrativeUnitInsertRegex();

    [GeneratedRegex(@"\('(\d{2})','((?:[^']|'')+)','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+',(\d+)\)")]
    private static partial Regex ProvinceRowRegex();

    [GeneratedRegex(@"\('(\d+)','((?:[^']|'')+)','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+','(\d+)',(\d+)\)")]
    private static partial Regex WardRowRegex();
}
