using Microsoft.Extensions.Logging;

namespace Verendar.Media.Infrastructure.Data.Seeders;

public static class OdometerTestMediaSeeder
{
    public static readonly Guid OdometerTestMediaId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-000000000001");

    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private const string OdometerDemoS3Key = "assets/OIP.webp";

    public static async Task SeedAsync(MediaDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var exists = await db.Files
            .IgnoreQueryFilters()
            .AnyAsync(f => f.Id == OdometerTestMediaId, cancellationToken);

        if (exists)
        {
            logger?.LogDebug("Odometer test media seed skipped: {MediaId} already exists", OdometerTestMediaId);
            return;
        }

        var now = DateTime.UtcNow;
        var entity = new MediaFile
        {
            Id = OdometerTestMediaId,
            UserId = TestUserId,
            Provider = StorageProvider.AwsS3,
            FileType = FileType.Other,
            FilePath = OdometerDemoS3Key,
            OriginalFileName = "OIP.webp",
            ContentType = "image/webp",
            Extension = ".webp",
            Size = 1,
            Status = FileStatus.Uploaded,
            Metadata = null,
            CreatedAt = now,
            CreatedBy = TestUserId
        };

        db.Files.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded odometer test media: Id={MediaId}, FilePath={FilePath}. Use mediaFileId in POST /api/v1/ai/odometer-scans.",
            OdometerTestMediaId,
            OdometerDemoS3Key);
    }
}
