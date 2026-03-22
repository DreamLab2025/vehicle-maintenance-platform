using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders
{
    public static class TestUserSeeder
    {
        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string TestUserEmail = "testuser@localhost.dev";
        private const string TestUserPassword = "Test@123";

        public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = EmailHelper.Normalize(TestUserEmail);
            var existing = await db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (existing != null)
            {
                logger?.LogDebug("Test user already exists: {Email}", normalizedEmail);
                return;
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = TestUserId,
                Email = normalizedEmail,
                FullName = "Test User (Background Jobs)",
                PasswordHash = string.Empty,
                EmailVerified = true,
                PhoneNumberVerified = false,
                Roles = new List<UserRole> { UserRole.User },
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            user.PasswordHash = hasher.HashPassword(user, TestUserPassword);

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Seeded test user: {Email} (Id: {UserId}) for background job flows", normalizedEmail, TestUserId);
        }
    }
}
