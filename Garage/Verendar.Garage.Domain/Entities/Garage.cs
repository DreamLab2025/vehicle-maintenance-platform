namespace Verendar.Garage.Domain.Entities;

[Index(nameof(OwnerId))]
[Index(nameof(Slug), IsUnique = true)]
public class Garage : BaseEntity
{
    public Guid OwnerId { get; set; }

    [Required, MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ShortName { get; set; }

    [Required, MaxLength(110)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TaxCode { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public GarageStatus Status { get; set; } = GarageStatus.Pending;

    public List<GarageBranch> Branches { get; set; } = [];
}
