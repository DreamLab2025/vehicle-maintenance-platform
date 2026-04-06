namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
public class GarageBundle : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public Money? DiscountAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
    public List<GarageBundleItem> Items { get; set; } = [];
}
