using Verendar.Common.Databases.Base;
using Verendar.Identity.Dtos;
using Verendar.Identity.Entities;

namespace Verendar.Identity.Mappings
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
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                EmailVerified = user.EmailVerified,
                PhoneNumberVerified = user.PhoneNumberVerified,
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
                FullName = request.Email,
                Status = EntityStatus.Active,
                Roles = new List<UserRole> { UserRole.User },
                PhoneNumberVerified = false,
                EmailVerified = false,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };
        }
    }
}
