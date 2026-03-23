using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
public class GarageProduct : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    /// <summary>Cross-service reference to Vehicle.PartCategory (ID only, no join).</summary>
    public Guid? PartCategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ProductType Type { get; set; }

    public Money Price { get; set; } = null!;

    public int? EstimatedDurationMinutes { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>JSON — compatible vehicle types.</summary>
    public string? CompatibleVehicleTypes { get; set; }

    /// <summary>Manufacturer km replacement interval (Part/Bundle only).</summary>
    public int? ManufacturerKmInterval { get; set; }

    /// <summary>Manufacturer month replacement interval (Part/Bundle only).</summary>
    public int? ManufacturerMonthInterval { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
}
