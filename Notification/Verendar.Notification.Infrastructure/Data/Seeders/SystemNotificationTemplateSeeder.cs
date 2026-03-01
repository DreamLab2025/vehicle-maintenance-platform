using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Seeders;

public static class SystemNotificationTemplateSeeder
{
    private static readonly Guid SystemUserId = Guid.Empty;

    private static readonly (Guid Id, string Code, string Title, string Message, NotificationType Type)[] TemplateData =
    {
        (Guid.Parse("10000000-0000-0000-0000-000000000001"), "OTP_PHONE_VERIFICATION", "Ma xac thuc SDT", "Ma xac thuc: {OTP}. Het han sau {ExpiryMinutes}p.", NotificationType.System),
        (Guid.Parse("10000000-0000-0000-0000-000000000003"), "OTP_PASSWORD_RESET", "Ma dat lai mat khau", "Ma reset pass: {OTP}. Het han sau {ExpiryMinutes}p.", NotificationType.System),
        (Guid.Parse("10000000-0000-0000-0000-000000000004"), "OTP_TWO_FACTOR", "Ma 2FA", "Ma 2FA: {OTP}.", NotificationType.System),
        (Guid.Parse("10000000-0000-0000-0000-000000000005"), "OTP_REQUESTED", "Ma OTP", "Ma OTP: {OTP}.", NotificationType.System),
        (Guid.Parse("20000000-0000-0000-0000-000000000001"), "WELCOME_USER", "Chao mung!", "Xin chao {UserName} den voi Verender!", NotificationType.Welcome),
        (Guid.Parse("20000000-0000-0000-0000-000000000002"), "PHONE_VERIFIED", "SDT da xac thuc", "SDT {PhoneNumber} da xac thuc.", NotificationType.Welcome),
        (Guid.Parse("30000000-0000-0000-0000-000000000001"), "PASSWORD_CHANGED", "Doi mat khau", "Mat khau da doi luc {ChangeTime}.", NotificationType.System),
        (Guid.Parse("30000000-0000-0000-0000-000000000002"), "ACCOUNT_LOCKED", "Tai khoan bi khoa", "Ly do: {Reason}.", NotificationType.System),
        (Guid.Parse("30000000-0000-0000-0000-000000000003"), "ACCOUNT_UNLOCKED", "Mo khoa tai khoan", "Tai khoan da duoc mo.", NotificationType.System),
    };

    public static async Task SeedAsync(NotificationDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var existingCodes = await db.NotificationTemplates
            .IgnoreQueryFilters()
            .Select(t => t.Code)
            .ToListAsync(cancellationToken);

        var toAdd = TemplateData.Where(t => !existingCodes.Contains(t.Code)).ToList();
        if (toAdd.Count == 0)
        {
            logger?.LogDebug("System notification templates already seeded, skipping");
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var (id, code, title, message, type) in toAdd)
        {
            var template = new NotificationTemplate
            {
                Id = id,
                Code = code,
                TitleTemplate = title,
                MessageTemplate = message,
                NotificationType = type,
                DefaultPriority = NotificationPriority.Medium,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = SystemUserId
            };
            db.NotificationTemplates.Add(template);

            db.NotificationTemplateChannels.Add(new NotificationTemplateChannel
            {
                Id = Guid.CreateVersion7(),
                NotificationTemplateId = id,
                Channel = NotificationChannel.SMS,
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = SystemUserId
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} system notification templates (SMS)", toAdd.Count);
    }
}
