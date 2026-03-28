using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders;

public static class GarageDevMemberUserSeeder
{
    private const string DevPassword = "12345@Abc";

    /// <summary>Audit CreatedBy — dev chủ garage (<see cref="GarageOwnerDevUserSeeder"/>).</summary>
    private static readonly Guid SeedActorId = GarageOwnerDevUserSeeder.UserId;

    public static async Task SeedAsync(UserDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var hasher = new PasswordHasher<User>();
        var added = 0;

        foreach (var def in DevMembers)
        {
            var normalizedEmail = EmailHelper.Normalize(def.Email);
            var exists = await db.Users
                .IgnoreQueryFilters()
                .AnyAsync(
                    u => u.Id == def.Id || u.Email == normalizedEmail,
                    cancellationToken);

            if (exists)
            {
                logger?.LogDebug(
                    "Garage dev Identity user skipped (exists): {Email} / {UserId}",
                    normalizedEmail,
                    def.Id);
                continue;
            }

            var user = new User
            {
                Id = def.Id,
                Email = normalizedEmail,
                FullName = def.FullName,
                PasswordHash = string.Empty,
                EmailVerified = true,
                PhoneNumberVerified = false,
                PhoneNumber = def.Phone,
                Roles = [def.Role],
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SeedActorId
            };
            user.PasswordHash = hasher.HashPassword(user, DevPassword);
            db.Users.Add(user);
            added++;
        }

        if (added == 0)
        {
            logger?.LogDebug("Garage dev Identity member users: nothing to add (all present)");
            return;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation(
            "Seeded {Count} garage dev member user(s) in Identity (password: dev default). Match GarageBranchDevSeeder UserIds.",
            added);
    }

    private readonly record struct MemberDef(
        Guid Id,
        string Email,
        string FullName,
        string? Phone,
        UserRole Role);

    /// <summary>Ids phải trùng <c>GarageBranchDevSeeder.DevUser*</c>.</summary>
    private static readonly MemberDef[] DevMembers =
    [
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            "manager.hk@verendar.dev",
            "Nguyễn Văn Minh",
            "0911110001",
            UserRole.GarageManager),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333302"),
            "mechanic1.hk@verendar.dev",
            "Trần Thị Lan",
            "0911110002",
            UserRole.Mechanic),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333303"),
            "mechanic2.hk@verendar.dev",
            "Lê Hoàng Nam",
            "0911110003",
            UserRole.Mechanic),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333311"),
            "manager.bd@verendar.dev",
            "Phạm Quốc Anh",
            "0922220001",
            UserRole.GarageManager),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333312"),
            "mechanic1.bd@verendar.dev",
            "Hoàng Thị Mai",
            "0922220002",
            UserRole.Mechanic),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333313"),
            "mechanic2.bd@verendar.dev",
            "Đỗ Văn Hùng",
            "0922220003",
            UserRole.Mechanic),
    ];
}
