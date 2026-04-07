namespace Verendar.Location.Infrastructure.Data.Seeders;

using Microsoft.Extensions.Logging;

public static class ProvinceBoundaryUrlSeeder
{
    private static readonly Dictionary<string, string> BoundaryUrls = new()
    {
        ["01"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/ha_noi.json",
        ["04"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/cao_bang.json",
        ["08"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/tuyen_quang.json",
        ["11"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/dien_bien.json",
        ["12"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/lai_chau.json",
        ["14"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/son_la.json",
        ["15"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/lao_cai.json",
        ["19"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/thai_nguyen.json",
        ["20"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/lang_son.json",
        ["22"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/quang_ninh.json",
        ["24"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/bac_ninh.json",
        ["25"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/phu_tho.json",
        ["31"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/hai_phong.json",
        ["33"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/hung_yen.json",
        ["37"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/ninh_binh.json",
        ["38"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/thanh_hoa.json",
        ["40"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/nghe_an.json",
        ["42"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/ha_tinh.json",
        ["44"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/quang_tri.json",
        ["46"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/hue.json",
        ["48"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/da_nang.json",
        ["51"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/quang_ngai.json",
        ["52"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/gia_lai.json",
        ["56"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/khanh_hoa.json",
        ["66"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/dak_lak.json",
        ["68"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/lam_dong.json",
        ["75"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/dong_nai.json",
        ["79"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/tp_ho_chi_minh.json",
        ["80"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/tay_ninh.json",
        ["82"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/dong_thap.json",
        ["86"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/vinh_long.json",
        ["91"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/an_giang.json",
        ["92"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/can_tho.json",
        ["96"] = "https://d3iova6424vljy.cloudfront.net/prod/location/boundaries/ca_mau.json",
    };

    public static async Task SeedAsync(LocationDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var existingCodes = await db.Provinces
            .Where(p => p.BoundaryUrl == null)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        if (existingCodes.Count == 0)
        {
            logger?.LogInformation("ProvinceBoundaryUrlSeeder: all provinces already have BoundaryUrl, skipping");
            return;
        }

        // Bulk UPDATE via VALUES table — replaces 34 individual round trips with 1 query
        // URLs are hardcoded constants — no SQL injection risk
        var valueRows = string.Join(",", BoundaryUrls.Select(kv => $"('{kv.Key}','{kv.Value}')"));
#pragma warning disable EF1002 // values are compile-time constants, no injection risk
        await db.Database.ExecuteSqlRawAsync(
            $"""
            UPDATE "Provinces" SET "BoundaryUrl" = v.url
            FROM (VALUES {valueRows}) AS v(code, url)
            WHERE "Code" = v.code AND "BoundaryUrl" IS NULL
            """,
            cancellationToken);
#pragma warning restore EF1002

        logger?.LogInformation("ProvinceBoundaryUrlSeeder: seeded {Count} province boundary URLs", BoundaryUrls.Count);
    }
}
