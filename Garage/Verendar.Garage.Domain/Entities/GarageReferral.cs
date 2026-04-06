namespace Verendar.Garage.Domain.Entities;

public class GarageReferral : BaseEntity
{
    public Guid GarageId { get; set; }

    public Guid ReferredUserId { get; set; }

    public DateTime ReferredAt { get; set; }

    [MaxLength(30)]
    public string ReferralCode { get; set; } = string.Empty;

    public Garage Garage { get; set; } = null!;
}
