using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Identity.Data;
using Verendar.Identity.Entities;

namespace Verendar.Identity.Seeders;

public static class TestUserSeeder
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string TestUserEmail = "hoalvpse181951@fpt.edu.vn";
    private const string TestUserPassword = "Test@123";

    public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var existing = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == TestUserEmail, cancellationToken);

        if (existing != null)
        {
            logger?.LogDebug("Test user already exists: {Email}", TestUserEmail);
            return;
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = TestUserId,
            Email = TestUserEmail,
            FullName = "Test User (Background Jobs)",
            PasswordHash = string.Empty,
            EmailVerified = true,
            PhoneNumberVerified = false,
            Status = EntityStatus.Active,
            Roles = new List<UserRole> { UserRole.User },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        user.PasswordHash = hasher.HashPassword(user, TestUserPassword);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded test user: {Email} (Id: {UserId}) for background job flows", TestUserEmail, TestUserId);
    }
}
