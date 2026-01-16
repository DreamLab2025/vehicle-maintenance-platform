using System.ComponentModel.DataAnnotations;
using Verender.Common.Databases.Base;

namespace Verender.Identity.Entities
{
    public class User : BaseEntity
    {
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

        public EntityStatus Status { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public enum UserRole
    {
        User = 1,
        Admin = 2
    }
}
