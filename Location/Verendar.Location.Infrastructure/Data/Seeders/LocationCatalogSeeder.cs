namespace Verendar.Location.Infrastructure.Data.Seeders;

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Orchestrates seeding of location reference data into database
/// Executes seeding in dependency order: Regions → Units → Provinces → Wards
/// Wards are parsed from embedded SQL resource (3,355 records from vietnamese-provinces-database)
/// Idempotent - only seeds if data doesn't exist
/// </summary>
public static partial class LocationCatalogSeeder
{
    private const string SeedSqlResourceName = "Verendar.Location.Infrastructure.Data.Seeders.seed_data.sql";
    private const int BatchSize = 500;

    public static async Task SeedAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken = default)
    {
        await SeedAdministrativeRegionsAsync(db, logger, cancellationToken);
        await SeedAdministrativeUnitsAsync(db, logger, cancellationToken);
        await SeedProvincesAsync(db, logger, cancellationToken);
        await SeedWardsFromSqlAsync(db, logger, cancellationToken);
    }

    private static async Task SeedAdministrativeRegionsAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        if (await db.AdministrativeRegions.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Administrative regions already seeded, skipping");
            return;
        }

        var regions = LocationDataSeeder.GetAdministrativeRegions();
        db.AdministrativeRegions.AddRange(regions);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} administrative regions", regions.Count);
    }

    private static async Task SeedAdministrativeUnitsAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        if (await db.AdministrativeUnits.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Administrative units already seeded, skipping");
            return;
        }

        var units = LocationDataSeeder.GetAdministrativeUnits();
        db.AdministrativeUnits.AddRange(units);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} administrative units", units.Count);
    }

    private static async Task SeedProvincesAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        if (await db.Provinces.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Provinces already seeded, skipping");
            return;
        }

        var provinces = LocationDataSeeder.GetProvinces();
        db.Provinces.AddRange(provinces);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} provinces", provinces.Count);
    }

    /// <summary>
    /// Seeds wards by parsing the embedded SQL resource file from vietnamese-provinces-database.
    /// Extracts code, name, province_code, administrative_unit_id from each ward INSERT row.
    /// Uses batch inserts (500 per batch) for performance.
    /// </summary>
    private static async Task SeedWardsFromSqlAsync(LocationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        if (await db.Wards.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Wards already seeded, skipping");
            return;
        }

        var sql = await ReadEmbeddedSqlAsync();
        if (string.IsNullOrEmpty(sql))
        {
            logger?.LogWarning("Seed SQL resource not found, falling back to hardcoded wards");
            var fallbackWards = LocationDataSeeder.GetWards();
            db.Wards.AddRange(fallbackWards);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Seeded {Count} wards (fallback)", fallbackWards.Count);
            return;
        }

        var wards = ParseWardsFromSql(sql);
        logger?.LogInformation("Parsed {Count} wards from SQL resource", wards.Count);

        // Batch insert for performance
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

    /// <summary>
    /// Parses ward entries from SQL INSERT statements.
    /// Format: ('code','name','name_en','full_name','full_name_en','code_name','province_code',unit_id)
    /// Extracts: code (1st), name (2nd), province_code (7th), administrative_unit_id (8th)
    /// </summary>
    private static List<Ward> ParseWardsFromSql(string sql)
    {
        var wards = new List<Ward>();
        var regex = WardRowRegex();

        foreach (Match match in regex.Matches(sql))
        {
            var code = match.Groups[1].Value;
            var name = match.Groups[2].Value.Replace("''", "'"); // Unescape SQL single quotes
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

    /// <summary>
    /// Regex pattern to match ward data rows in the SQL INSERT statements.
    /// Captures: group 1 = code, group 2 = name, group 3 = province_code, group 4 = administrative_unit_id
    /// Skips name_en, full_name, full_name_en, code_name fields.
    /// </summary>
    [GeneratedRegex(@"\('(\d+)','((?:[^']|'')+)','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+','(?:[^']|'')+','(\d+)',(\d+)\)")]
    private static partial Regex WardRowRegex();
}
