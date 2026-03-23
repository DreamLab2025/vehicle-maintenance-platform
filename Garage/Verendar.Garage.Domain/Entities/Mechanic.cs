namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
[Index(nameof(UserId))]
public class Mechanic : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    /// <summary>FK → Identity User (role: Mechanic).</summary>
    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public MechanicStatus Status { get; set; } = MechanicStatus.Active;

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
}
