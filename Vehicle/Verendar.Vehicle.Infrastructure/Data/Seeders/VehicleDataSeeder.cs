using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Infrastructure.Data;

public static class VehicleDataSeeder
{
    private static readonly DateTime FixedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid SystemUserId = Guid.Empty;

    private static Guid G(IReadOnlyDictionary<string, string> row, string key) =>
        Guid.TryParse(Get(row, key), out var g) ? g : Guid.Empty;

    private static string Get(IReadOnlyDictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var v) ? (v ?? "") : "";

    private static int Int(IReadOnlyDictionary<string, string> row, string key, int def = 0) =>
        int.TryParse(Get(row, key), out var n) ? n : def;

    private static decimal Dec(IReadOnlyDictionary<string, string> row, string key, decimal def = 0) =>
        decimal.TryParse(Get(row, key), out var d) ? d : def;

    private static bool Bool(IReadOnlyDictionary<string, string> row, string key)
    {
        var v = Get(row, key).Trim().ToUpperInvariant();
        return v == "1" || v == "TRUE" || v == "YES";
    }

    private static TEntity BaseAudit<TEntity>(TEntity e) where TEntity : BaseEntity
    {
        e.CreatedAt = FixedDate;
        e.CreatedBy = SystemUserId;
        return e;
    }

    public static List<VehicleType> GetVehicleTypes()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleTypes.csv");
        return rows.Select(row => BaseAudit(new VehicleType
        {
            Id = G(row, "Id"),
            Name = Get(row, "Name"),
            Code = Get(row, "Code"),
            Description = Get(row, "Description").NullIfEmpty(),
            ImageUrl = Get(row, "ImageUrl").NullIfEmpty(),
        })).ToList();
    }

    public static List<Brand> GetVehicleBrands()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleBrands.csv");
        return rows.Select(row => BaseAudit(new Brand
        {
            Id = G(row, "Id"),
            VehicleTypeId = G(row, "VehicleTypeId"),
            Name = Get(row, "Name"),
            Code = Get(row, "Code"),
            LogoUrl = Get(row, "LogoUrl").NullIfEmpty(),
            Website = Get(row, "Website").NullIfEmpty(),
            SupportPhone = Get(row, "SupportPhone").NullIfEmpty(),
        })).ToList();
    }

    public static List<Model> GetVehicleModels()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleModels.csv");
        return rows.Select(row => BaseAudit(new Model
        {
            Id = G(row, "Id"),
            VehicleBrandId = G(row, "VehicleBrandId"),
            Name = Get(row, "Name"),
            Code = Get(row, "Code"),
            ManufactureYear = Int(row, "ManufactureYear") is var y && y > 0 ? y : null,
            FuelType = Int(row, "FuelType") is var f && f > 0 ? (VehicleFuelType?)f : null,
            TransmissionType = Int(row, "TransmissionType") is var t && t > 0 ? (VehicleTransmissionType?)t : null,
            EngineDisplacement = Int(row, "EngineDisplacement") is var d && d > 0 ? d : null,
            EngineCapacity = Dec(row, "EngineCapacity") > 0 ? Dec(row, "EngineCapacity") : null,
            Description = Get(row, "Description").NullIfEmpty(),
        })).ToList();
    }

    public static List<Variant> GetVehicleVariants()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleVariants.csv");
        return rows
            .Where(row => !string.IsNullOrWhiteSpace(Get(row, "ImageUrl")))
            .Select(row => BaseAudit(new Variant
            {
                Id = G(row, "Id"),
                VehicleModelId = G(row, "VehicleModelId"),
                Color = Get(row, "Color"),
                HexCode = Get(row, "HexCode"),
                ImageUrl = Get(row, "ImageUrl")
            }))
            .ToList();
    }

    public static List<PartCategory> GetPartCategories()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("PartCategories.csv");
        return rows.Select(row => BaseAudit(new PartCategory
        {
            Id = G(row, "Id"),
            Name = Get(row, "Name"),
            Code = Get(row, "Code"),
            Description = Get(row, "Description").NullIfEmpty(),
            IconUrl = Get(row, "IconUrl").NullIfEmpty(),
            DisplayOrder = Int(row, "DisplayOrder"),
            RequiresOdometerTracking = Bool(row, "RequiresOdometerTracking"),
            RequiresTimeTracking = Bool(row, "RequiresTimeTracking"),
            AllowsMultipleInstances = Bool(row, "AllowsMultipleInstances"),
            IdentificationSigns = Get(row, "IdentificationSigns").NullIfEmpty(),
            ConsequencesIfNotHandled = Get(row, "ConsequencesIfNotHandled").NullIfEmpty()
        })).ToList();
    }

    public static List<DefaultMaintenanceSchedule> GetDefaultMaintenanceSchedules()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("DefaultMaintenanceSchedules.csv");
        return rows.Select(row => BaseAudit(new DefaultMaintenanceSchedule
        {
            Id = G(row, "Id"),
            VehicleModelId = G(row, "VehicleModelId"),
            PartCategoryId = G(row, "PartCategoryId"),
            InitialKm = Int(row, "InitialKm"),
            KmInterval = Int(row, "KmInterval"),
            MonthsInterval = Int(row, "MonthsInterval"),
        })).ToList();
    }

    public static List<PartProduct> GetPartProducts()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("PartProducts.csv");
        return rows
            .Where(row => !string.IsNullOrWhiteSpace(Get(row, "ImageUrl")))
            .Select(row => BaseAudit(new PartProduct
            {
                Id = G(row, "Id"),
                PartCategoryId = G(row, "PartCategoryId"),
                Name = Get(row, "Name"),
                Brand = Get(row, "Brand").NullIfEmpty(),
                Description = Get(row, "Description").NullIfEmpty(),
                ImageUrl = Get(row, "ImageUrl").NullIfEmpty(),
                ReferencePrice = Dec(row, "ReferencePrice") > 0 ? Dec(row, "ReferencePrice") : null,
                RecommendedKmInterval = Int(row, "RecommendedKmInterval") > 0 ? Int(row, "RecommendedKmInterval") : null,
                RecommendedMonthsInterval = Int(row, "RecommendedMonthsInterval") > 0 ? Int(row, "RecommendedMonthsInterval") : null,
            }))
            .ToList();
    }

    private static string? NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
