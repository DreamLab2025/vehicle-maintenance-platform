using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
public class GarageService : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    public Guid? ServiceCategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public Money LaborPrice { get; set; } = null!;

    public int? EstimatedDurationMinutes { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
    public ServiceCategory? ServiceCategory { get; set; }
}
