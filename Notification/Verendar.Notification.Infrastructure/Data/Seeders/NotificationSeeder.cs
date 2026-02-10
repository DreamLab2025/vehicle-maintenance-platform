using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Infrastructure.Data;
using NotificationEntity = Verendar.Notification.Domain.Entities.Notification;

namespace Verendar.Notification.Infrastructure.Seeders;

public static class NotificationSeeder
{
    private static readonly Guid SystemUserId = Guid.Empty;
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string TestUserEmail = "hoalvpse181951@fpt.edu.vn";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly (string Title, string Message, NotificationType Type, NotificationPriority Priority, string EntityType, int? Level, string? LevelName)[] NotificationItems =
    {
        ("Khẩn cấp: Cần thay linh kiện",
            "Xe cua ban co linh kien da den muc khan cap can thay the. Cac linh kien can chu y:\n• Dau nhot dong co (so km hien tai: 5,000, can thay truoc: 6,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 4, "Urgent"),
        ("Nhắc nhở bảo dưỡng (High)",
            "Xe cua ban co linh kien can chu y bao duong/thay the:\n• Lop xe (so km hien tai: 15,000, can thay truoc: 20,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 3, "High"),
        ("Nhắc nhở bảo dưỡng (Medium)",
            "Xe cua ban co linh kien can chu y bao duong/thay the:\n• Ma phanh (so km hien tai: 7,000, can thay truoc: 10,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 2, "Medium"),
        ("Nhắc nhở cập nhật số km",
            "Xe 59-TEST-01 chua cap nhat so km trong 5 ngay qua. Vui long vao app cap nhat so km de duoc nhac bao duong chinh xac.",
            NotificationType.OdometerReminder, NotificationPriority.Medium, "OdometerReminder", null, null),
    };

    public static async Task SeedAsync(NotificationDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        await SeedPreferenceAsync(db, logger, cancellationToken);
        await SeedNotificationsAsync(db, logger, cancellationToken);
    }

    private static async Task SeedPreferenceAsync(NotificationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        var existing = await db.NotificationPreferences
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == TestUserId, cancellationToken);
        if (existing != null)
        {
            logger?.LogDebug("Notification preference already exists for test user {UserId}", TestUserId);
            return;
        }
        var preference = new NotificationPreference
        {
            Id = Guid.CreateVersion7(),
            UserId = TestUserId,
            Email = TestUserEmail,
            EmailVerified = true,
            PhoneNumber = null,
            PhoneNumberVerified = false,
            InAppEnabled = true,
            SmsEnabled = true,
            SmsForHighPriorityOnly = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.NotificationPreferences.Add(preference);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded notification preference for test user (UserId: {UserId}, Email: {Email})", TestUserId, TestUserEmail);
    }

    private static async Task SeedNotificationsAsync(NotificationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        var existingCount = await db.Notifications
            .IgnoreQueryFilters()
            .CountAsync(n => n.UserId == TestUserId && (n.EntityType == "MaintenanceReminder" || n.EntityType == "OdometerReminder"), cancellationToken);
        if (existingCount > 0)
        {
            logger?.LogDebug("Test user already has {Count} vehicle-related notifications for user {UserId}, skip seed", existingCount, TestUserId);
            return;
        }

        foreach (var (title, message, type, priority, entityType, level, levelName) in NotificationItems)
        {
            var metadata = level.HasValue
                ? JsonSerializer.Serialize(new { Source = "MaintenanceReminder", Level = level, LevelName = levelName }, JsonOptions)
                : JsonSerializer.Serialize(new { Source = "OdometerReminder" }, JsonOptions);

            var notification = new NotificationEntity
            {
                Id = Guid.CreateVersion7(),
                UserId = TestUserId,
                Title = title,
                Message = message,
                NotificationType = type,
                Priority = priority,
                Status = NotificationStatus.Sent,
                EntityType = entityType,
                MetadataJson = metadata,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemUserId
            };
            db.Notifications.Add(notification);

            var inAppDelivery = new NotificationDelivery
            {
                Id = Guid.CreateVersion7(),
                NotificationId = notification.Id,
                Channel = NotificationChannel.InApp,
                RecipientAddress = TestUserId.ToString(),
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                DeliveredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemUserId
            };
            db.NotificationDeliveries.Add(inAppDelivery);

            var emailDelivery = new NotificationDelivery
            {
                Id = Guid.CreateVersion7(),
                NotificationId = notification.Id,
                Channel = NotificationChannel.EMAIL,
                RecipientAddress = TestUserEmail,
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                DeliveredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemUserId
            };
            db.NotificationDeliveries.Add(emailDelivery);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} notifications for test user (UserId: {UserId}) corresponding to user vehicle", NotificationItems.Length, TestUserId);
    }
}
