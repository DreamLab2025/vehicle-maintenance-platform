namespace Verendar.Garage.Application.Dtos;

// ── Requests ──────────────────────────────────────────────────────────────────

public class CreateGarageProductRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto MaterialPrice { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? CompatibleVehicleTypes { get; set; }
    public int? ManufacturerKmInterval { get; set; }
    public int? ManufacturerMonthInterval { get; set; }
    public Guid? PartCategoryId { get; set; }
    public Guid? InstallationServiceId { get; set; }
}

public class UpdateGarageProductRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto MaterialPrice { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? CompatibleVehicleTypes { get; set; }
    public int? ManufacturerKmInterval { get; set; }
    public int? ManufacturerMonthInterval { get; set; }
    public Guid? PartCategoryId { get; set; }
    public Guid? InstallationServiceId { get; set; }
}

public class UpdateGarageProductStatusRequest
{
    public ProductStatus Status { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────

public class GarageProductListItemResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto MaterialPrice { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? PartCategoryId { get; set; }
    public bool HasInstallationOption { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GarageProductResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto MaterialPrice { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? CompatibleVehicleTypes { get; set; }
    public int? ManufacturerKmInterval { get; set; }
    public int? ManufacturerMonthInterval { get; set; }
    public Guid? PartCategoryId { get; set; }
    public ProductStatus Status { get; set; }
    public InstallationServiceSummary? InstallationService { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InstallationServiceSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public MoneyDto LaborPrice { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
}
