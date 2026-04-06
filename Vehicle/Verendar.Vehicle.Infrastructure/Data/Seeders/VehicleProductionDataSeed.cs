namespace Verendar.Vehicle.Infrastructure.Seeders;


public static partial class VehicleProductionDataSeed
{
    public static async Task SeedAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        await SeedVehicleTypesAsync(db, logger, cancellationToken);
        await SeedVehicleBrandsAsync(db, logger, cancellationToken);
        await SeedVehicleModelsAsync(db, logger, cancellationToken);
        await SeedVehicleVariantsAsync(db, logger, cancellationToken);
        await SeedPartCategoriesAsync(db, logger, cancellationToken);
        await SeedDefaultMaintenanceSchedulesAsync(db, logger, cancellationToken);
        await SeedQuestionnaireAsync(db, logger, cancellationToken);
    }

    private static async Task SeedVehicleTypesAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.VehicleTypes.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadVehicleTypesFromCsv();
        db.VehicleTypes.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} VehicleTypes", list.Count);
    }

    private static async Task SeedVehicleBrandsAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.VehicleBrands.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadVehicleBrandsFromCsv();
        db.VehicleBrands.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} VehicleBrands", list.Count);
    }

    private static async Task SeedVehicleModelsAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.VehicleModels.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadVehicleModelsFromCsv();
        db.VehicleModels.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} VehicleModels", list.Count);
    }

    private static async Task SeedVehicleVariantsAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.VehicleVariants.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadVehicleVariantsFromCsv();
        db.VehicleVariants.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} VehicleVariants", list.Count);
    }

    private static async Task SeedPartCategoriesAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.PartCategories.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadPartCategoriesFromCsv();
        db.PartCategories.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} PartCategories", list.Count);
    }

    private static async Task SeedDefaultMaintenanceSchedulesAsync(VehicleDbContext db, ILogger? logger, CancellationToken ct)
    {
        var hasAny = await db.DefaultMaintenanceSchedules.IgnoreQueryFilters().AnyAsync(ct);
        if (hasAny) return;
        var list = LoadDefaultMaintenanceSchedulesFromCsv();
        db.DefaultMaintenanceSchedules.AddRange(list);
        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} DefaultMaintenanceSchedules", list.Count);
    }
}
