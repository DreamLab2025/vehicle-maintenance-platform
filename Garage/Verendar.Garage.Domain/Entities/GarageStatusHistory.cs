namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageId))]
public class GarageStatusHistory : BaseEntity
{
    public Guid GarageId { get; set; }

    public GarageStatus FromStatus { get; set; }

    public GarageStatus ToStatus { get; set; }

    public Guid ChangedByUserId { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }

    // Navigation
    public Garage Garage { get; set; } = null!;
}
