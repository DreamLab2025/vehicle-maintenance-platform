using System.ComponentModel.DataAnnotations;
using VMP.Common.Databases.Interfaces;

namespace VMP.Identity.Entities
{
    public class User : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public List<UserRole> Roles { get; set; } = [];
        public UserStatus Status { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum UserStatus
    {
        Inactive = 0,
        Active = 1,
        Suspended = 2,
        Deleted = 3
    }

    public enum UserRole
    {
        User = 1,
        Admin = 2
    }
}
