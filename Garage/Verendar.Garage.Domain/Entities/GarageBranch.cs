using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageId))]
public class GarageBranch : BaseEntity
{
    public Guid GarageId { get; set; }

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

    [MaxLength(20)]
    public string? TaxCode { get; set; } //Default is TaxCode of Garage

    public BranchStatus Status { get; set; } = BranchStatus.Active;

    // Navigation
    public Garage Garage { get; set; } = null!;
    public List<GarageProduct> Products { get; set; } = [];
    public List<GarageMember> Members { get; set; } = [];
    public List<GarageReview> Reviews { get; set; } = [];
}
