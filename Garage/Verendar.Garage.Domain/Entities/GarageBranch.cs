using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageAccountId))]
public class GarageBranch : BaseEntity
{
    public Guid GarageAccountId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public Address Address { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public WorkingHours WorkingHours { get; set; } = null!;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public BranchStatus Status { get; set; } = BranchStatus.Active;

    // Navigation
    public GarageAccount GarageAccount { get; set; } = null!;
    public List<GarageProduct> Products { get; set; } = [];
    public List<Mechanic> Mechanics { get; set; } = [];
    public List<GarageReview> Reviews { get; set; } = [];
}
