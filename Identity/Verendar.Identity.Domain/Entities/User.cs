namespace Verendar.Identity.Domain.Entities
{
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

        public DateOnly? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public List<UserRole> Roles { get; set; } = [];

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public enum UserRole
    {
        User = 1,
        Admin = 2,
        GarageOwner = 3,
        Mechanic = 4,
        GarageManager = 5
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }
}
