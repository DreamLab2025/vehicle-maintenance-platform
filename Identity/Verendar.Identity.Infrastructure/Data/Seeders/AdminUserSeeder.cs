using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders
{
    public static class AdminUserSeeder
    {
        private static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static async Task SeedAsync(
            UserDbContext db,
            IConfiguration configuration,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            var email = configuration["Seed:Admin:Email"] ?? "admin@gmail.com";
            var password = configuration["Seed:Admin:Password"] ?? "12345@Abc";
            var fullName = configuration["Seed:Admin:FullName"] ?? "System Administrator";

            var normalizedEmail = EmailHelper.Normalize(email);
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
                FullName = fullName,
                PasswordHash = string.Empty,
                EmailVerified = true,
                PhoneNumberVerified = false,
                Roles = [UserRole.Admin],
                CreatedAt = DateTime.UtcNow,
                CreatedBy = AdminUserId
            };
            user.PasswordHash = hasher.HashPassword(user, password);

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogWarning(
                "Seeded admin user: {Email} (Id: {UserId})",
                normalizedEmail,
                AdminUserId);
        }
    }
}
