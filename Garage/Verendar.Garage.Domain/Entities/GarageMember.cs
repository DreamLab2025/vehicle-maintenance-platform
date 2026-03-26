namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
[Index(nameof(UserId))]
public class GarageMember : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    public Guid UserId { get; set; }

    public MemberRole Role { get; set; }

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
}
