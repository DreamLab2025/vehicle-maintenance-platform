using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Seeders;

/// <summary>
/// Seed notification preference cho test account (UserId trùng với Vehicle service) để nhận reminder (MaintenanceReminder, OdometerReminder).
/// </summary>
public static class NotificationPreferenceSeeder
{
    public static async Task SeedAsync(NotificationDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var userId = NotificationPreferenceSeedData.TestUserId;

        var existing = await db.NotificationPreferences
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (existing != null)
        {
            logger?.LogDebug("Notification preference already exists for test user {UserId}", userId);
            return;
        }

        var preference = new NotificationPreference
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Email = NotificationPreferenceSeedData.TestUserEmail,
            EmailVerified = true,
            PhoneNumber = NotificationPreferenceSeedData.TestUserPhone,
            PhoneNumberVerified = false,
            InAppEnabled = true,
            SmsEnabled = true,
            SmsForHighPriorityOnly = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        db.NotificationPreferences.Add(preference);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded notification preference for test user (UserId: {UserId}, Email: {Email})",
            userId, preference.Email);
    }
}
