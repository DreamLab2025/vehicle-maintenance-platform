using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Identity.Application.Helpers;

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
            var existing = await db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (existing != null)
            {
                logger?.LogDebug("Admin user already exists: {Email}", normalizedEmail);
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
                Status = EntityStatus.Active,
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
