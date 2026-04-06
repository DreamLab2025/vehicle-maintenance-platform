namespace Verendar.Garage.Domain.Entities;

[Index(nameof(Slug), IsUnique = true)]
[Index(nameof(DisplayOrder))]
public class ServiceCategory : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; }

    // Navigation
    public List<GarageService> Services { get; set; } = [];
}
