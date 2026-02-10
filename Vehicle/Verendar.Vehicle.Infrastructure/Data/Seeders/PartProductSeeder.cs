using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Seeders;

public static class PartProductSeeder
{
    public static async Task SeedAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var existingCount = await db.PartProducts
            .IgnoreQueryFilters()
            .CountAsync(cancellationToken);

        if (existingCount > 0)
        {
            logger?.LogDebug("PartProducts already seeded ({Count} products), skipping", existingCount);
            return;
        }

        var products = VehicleDataSeeder.GetPartProducts();
        db.PartProducts.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation("Seeded {Count} PartProducts", products.Count);
    }
}
