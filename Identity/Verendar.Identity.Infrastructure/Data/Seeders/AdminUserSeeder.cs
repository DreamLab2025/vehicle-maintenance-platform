using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders
{
    public static class AdminUserSeeder
    {
        private static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private const string AdminEmail = "admin@gmail.com";
        private const string AdminPassword = "12345@Abc";

        public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = EmailHelper.Normalize(AdminEmail);
            var alreadySeeded = await db.Users
                .IgnoreQueryFilters()
                .AnyAsync(
                    u => u.Id == AdminUserId || u.Email == normalizedEmail,
                    cancellationToken);

            if (alreadySeeded)
            {
                logger?.LogDebug(
                    "Admin seed skipped: row already exists for Id {UserId} or email {Email}",
                    AdminUserId,
                    normalizedEmail);
                return;
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = AdminUserId,
                Email = normalizedEmail,
                FullName = "System Administrator (Dev Seed)",
                PasswordHash = string.Empty,
                EmailVerified = true,
                PhoneNumberVerified = false,
                Roles = [UserRole.Admin],
                CreatedAt = DateTime.UtcNow,
                CreatedBy = AdminUserId
            };
            user.PasswordHash = hasher.HashPassword(user, AdminPassword);

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation(
                "Seeded admin user: {Email} (Id: {UserId}) — Development only",
                normalizedEmail,
                AdminUserId);
        }
    }
}
