using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Identity.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public bool EmailVerified { get; set; } = false;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberVerified { get; set; } = false;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RefreshToken { get; set; }

    public List<UserRole> Roles { get; set; } = [];

    public EntityStatus Status { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}

public enum UserRole
{
    User = 1,
    Admin = 2
}
