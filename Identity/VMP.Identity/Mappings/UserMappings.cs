using VMP.Identity.Dtos;
using VMP.Identity.Entities;

namespace VMP.Identity.Mappings
{
    public static class UserMappings
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.IsEmailConfirmed,
                IsPhoneNumberConfirmed = user.IsPhoneNumberConfirmed,
                Status = user.Status.ToString(),
                Roles = user.Roles.Select(r => r.ToString()).ToList(),
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this RegisterRequest request, string passwordHash)
        {
            var userId = Guid.CreateVersion7();
            return new User
            {
                Id = userId,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.Email.Split('@')[0],
                Status = UserStatus.Active,
                Roles = new List<UserRole> { UserRole.User },
                IsEmailConfirmed = false,
                IsPhoneNumberConfirmed = false,
                PhoneNumber = string.Empty,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
        }
    }
}
