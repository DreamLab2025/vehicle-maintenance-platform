using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders;

public static class GarageOwnerDevUserSeeder
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111222");

    private const string Email = "garage@gmail.com";
    private const string Password = "12345@Abc";

    public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = EmailHelper.Normalize(Email);
        var alreadySeeded = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == UserId || u.Email == normalizedEmail, cancellationToken);

        if (alreadySeeded)
        {
            logger?.LogDebug("Garage owner dev user skipped (exists): {Email} / {UserId}", normalizedEmail, UserId);
            return;
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = UserId,
            Email = normalizedEmail,
            FullName = "Garage Owner (Dev)",
            PasswordHash = string.Empty,
            EmailVerified = true,
            PhoneNumberVerified = false,
            Roles = [UserRole.User, UserRole.GarageOwner],
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserId
        };
        user.PasswordHash = hasher.HashPassword(user, Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded dev garage owner: {Email} (Id: {UserId})", normalizedEmail, UserId);
    }
}
