using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Infrastructure.Seeders;

public static partial class VehicleProductionDataSeed
{
    private static readonly DateTime CatalogCsvFixedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid CatalogCsvSystemUserId = Guid.Empty;

    private static Guid CsvGuid(IReadOnlyDictionary<string, string> row, string key) =>
        Guid.TryParse(CsvGet(row, key), out var g) ? g : Guid.Empty;

    private static string CsvGet(IReadOnlyDictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var v) ? (v ?? "") : "";

    private static int CsvInt(IReadOnlyDictionary<string, string> row, string key, int def = 0) =>
        int.TryParse(CsvGet(row, key), out var n) ? n : def;

    private static decimal CsvDec(IReadOnlyDictionary<string, string> row, string key, decimal def = 0) =>
        decimal.TryParse(CsvGet(row, key), out var d) ? d : def;

    private static bool CsvBool(IReadOnlyDictionary<string, string> row, string key)
    {
        var v = CsvGet(row, key).Trim().ToUpperInvariant();
        return v == "1" || v == "TRUE" || v == "YES";
    }

    private static TEntity WithCatalogAudit<TEntity>(TEntity e) where TEntity : BaseEntity
    {
        e.CreatedAt = CatalogCsvFixedDate;
        e.CreatedBy = CatalogCsvSystemUserId;
        return e;
    }

    private static string? NullIfEmptyCsv(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static List<VehicleType> LoadVehicleTypesFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleTypes.csv");
        return rows.Select(row => WithCatalogAudit(new VehicleType
        {
            Id = CsvGuid(row, "Id"),
            Name = CsvGet(row, "Name"),
            Slug = CsvGet(row, "Slug"),
            Description = NullIfEmptyCsv(CsvGet(row, "Description")),
            ImageUrl = NullIfEmptyCsv(CsvGet(row, "ImageUrl")),
        })).ToList();
    }

    private static List<Brand> LoadVehicleBrandsFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleBrands.csv");
        return rows.Select(row => WithCatalogAudit(new Brand
        {
            Id = CsvGuid(row, "Id"),
            VehicleTypeId = CsvGuid(row, "VehicleTypeId"),
            Name = CsvGet(row, "Name"),
            Slug = CsvGet(row, "Slug"),
            LogoUrl = NullIfEmptyCsv(CsvGet(row, "LogoUrl")),
            Website = NullIfEmptyCsv(CsvGet(row, "Website")),
            SupportPhone = NullIfEmptyCsv(CsvGet(row, "SupportPhone")),
        })).ToList();
    }

    private static List<Model> LoadVehicleModelsFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleModels.csv");
        return rows.Select(row => WithCatalogAudit(new Model
        {
            Id = CsvGuid(row, "Id"),
            VehicleBrandId = CsvGuid(row, "VehicleBrandId"),
            Name = CsvGet(row, "Name"),
            Slug = CsvGet(row, "Slug"),
            ManufactureYear = CsvInt(row, "ManufactureYear") is var y && y > 0 ? y : null,
            FuelType = CsvInt(row, "FuelType") is var f && f > 0 ? (VehicleFuelType?)f : null,
            TransmissionType = CsvInt(row, "TransmissionType") is var t && t > 0 ? (VehicleTransmissionType?)t : null,
            EngineDisplacement = CsvInt(row, "EngineDisplacement") is var d && d > 0 ? d : null,
            EngineCapacity = CsvDec(row, "EngineCapacity") > 0 ? CsvDec(row, "EngineCapacity") : null,
            Description = NullIfEmptyCsv(CsvGet(row, "Description")),
        })).ToList();
    }

    private static List<Variant> LoadVehicleVariantsFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("VehicleVariants.csv");
        return rows
            .Where(row => !string.IsNullOrWhiteSpace(CsvGet(row, "ImageUrl")))
            .Select(row => WithCatalogAudit(new Variant
            {
                Id = CsvGuid(row, "Id"),
                VehicleModelId = CsvGuid(row, "VehicleModelId"),
                Color = CsvGet(row, "Color"),
                HexCode = CsvGet(row, "HexCode"),
                ImageUrl = CsvGet(row, "ImageUrl")
            }))
            .ToList();
    }

    private static List<PartCategory> LoadPartCategoriesFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("PartCategories.csv");
        return rows.Select(row => WithCatalogAudit(new PartCategory
        {
            Id = CsvGuid(row, "Id"),
            Name = CsvGet(row, "Name"),
            Slug = CsvGet(row, "Slug"),
            Description = NullIfEmptyCsv(CsvGet(row, "Description")),
            IconUrl = NullIfEmptyCsv(CsvGet(row, "IconUrl")),
            DisplayOrder = CsvInt(row, "DisplayOrder"),
            RequiresOdometerTracking = CsvBool(row, "RequiresOdometerTracking"),
            RequiresTimeTracking = CsvBool(row, "RequiresTimeTracking"),
            AllowsMultipleInstances = CsvBool(row, "AllowsMultipleInstances"),
            IdentificationSigns = NullIfEmptyCsv(CsvGet(row, "IdentificationSigns")),
            ConsequencesIfNotHandled = NullIfEmptyCsv(CsvGet(row, "ConsequencesIfNotHandled"))
        })).ToList();
    }

    private static List<DefaultMaintenanceSchedule> LoadDefaultMaintenanceSchedulesFromCsv()
    {
        var rows = SeedDataLoader.ReadCsvAsDictionaries("DefaultMaintenanceSchedules.csv");
        return rows.Select(row => WithCatalogAudit(new DefaultMaintenanceSchedule
        {
            Id = CsvGuid(row, "Id"),
            VehicleModelId = CsvGuid(row, "VehicleModelId"),
            PartCategoryId = CsvGuid(row, "PartCategoryId"),
            InitialKm = CsvInt(row, "InitialKm"),
            KmInterval = CsvInt(row, "KmInterval"),
            MonthsInterval = CsvInt(row, "MonthsInterval"),
        })).ToList();
    }
}
