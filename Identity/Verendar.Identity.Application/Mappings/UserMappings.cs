namespace Verendar.Identity.Application.Mappings
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
                Roles = user.Roles.Select(r => r.ToString()).ToList(),
                CreatedAt = user.CreatedAt
            };
        }

        public static User ToEntity(this RegisterRequest request, string passwordHash)
        {
            var normalizedEmail = EmailHelper.Normalize(request.Email);
            var userId = Guid.CreateVersion7();
            return new User
            {
                Id = userId,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                FullName = normalizedEmail,
                Roles = new List<UserRole> { UserRole.User },
                PhoneNumberVerified = false,
                EmailVerified = false,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };
        }
    }
}
