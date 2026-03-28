using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders
{
    /// <summary>Khách hàng / user demo — không phải chủ garage (xem <see cref="GarageOwnerDevUserSeeder"/>).</summary>
    public static class TestUserSeeder
    {
        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string TestUserEmail = "user@gmail.com";
        private const string TestUserPassword = "12345@Abc";

        public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = EmailHelper.Normalize(TestUserEmail);
            var alreadySeeded = await db.Users
                .IgnoreQueryFilters()
                .AnyAsync(
                    u => u.Id == TestUserId || u.Email == normalizedEmail,
                    cancellationToken);

            if (alreadySeeded)
            {
                logger?.LogDebug(
                    "Test user seed skipped: row already exists for Id {UserId} or email {Email}",
                    TestUserId,
                    normalizedEmail);
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
