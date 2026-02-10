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

    private static readonly Guid SeedUserVehicleId = Guid.Parse("f0000001-0000-0000-0000-000000000001");
    private static readonly Guid[] SeedReminderIds = { Guid.Parse("f0000002-0000-0000-0000-000000000002"), Guid.Parse("f0000003-0000-0000-0000-000000000003"), Guid.Parse("f0000004-0000-0000-0000-000000000004") };
    private const string VehicleDisplayName = "59-TEST-01";

    private static readonly (string Title, string Message, NotificationType Type, NotificationPriority Priority, string EntityType, int? Level, string? LevelName, string PartCategoryName, int CurrentOdo, int TargetOdo, decimal PctRemaining)[] NotificationItems =
    {
        ("Khẩn cấp: Cần thay linh kiện",
            "Xe cua ban co linh kien da den muc khan cap can thay the. Cac linh kien can chu y:\n• Dau nhot dong co (so km hien tai: 5,000, can thay truoc: 6,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 4, "Urgent", "Dầu nhớt động cơ", 5000, 6000, 5m),
        ("Nhắc nhở bảo dưỡng (High)",
            "Xe cua ban co linh kien can chu y bao duong/thay the:\n• Lop xe (so km hien tai: 15,000, can thay truoc: 20,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 3, "High", "Lốp xe", 15000, 20000, 25m),
        ("Nhắc nhở bảo dưỡng (Medium)",
            "Xe cua ban co linh kien can chu y bao duong/thay the:\n• Ma phanh (so km hien tai: 7,000, can thay truoc: 10,000)",
            NotificationType.MaintenanceReminder, NotificationPriority.High, "MaintenanceReminder", 2, "Medium", "Má phanh", 7000, 10000, 30m),
        ("Nhắc nhở cập nhật số km",
            "Xe 59-TEST-01 chua cap nhat so km trong 5 ngay qua. Vui long vao app cap nhat so km de duoc nhac bao duong chinh xac.",
            NotificationType.OdometerReminder, NotificationPriority.Medium, "OdometerReminder", null, null, "", 0, 0, 0m),
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

        for (var i = 0; i < NotificationItems.Length; i++)
        {
            var (title, message, type, priority, entityType, level, levelName, partCategoryName, currentOdo, targetOdo, pctRemaining) = NotificationItems[i];
            string metadataJson;
            Guid? entityId;

            if (type == NotificationType.MaintenanceReminder && level.HasValue && levelName != null)
            {
                var reminderId = SeedReminderIds[i];
                var items = new[]
                {
                    new
                    {
                        partCategoryName,
                        userVehicleId = SeedUserVehicleId,
                        reminderId,
                        currentOdometer = currentOdo,
                        targetOdometer = targetOdo,
                        initialOdometer = currentOdo - 3000,
                        percentageRemaining = pctRemaining,
                        vehicleDisplayName = VehicleDisplayName
                    }
                };
                metadataJson = JsonSerializer.Serialize(new
                {
                    type = "MaintenanceReminder",
                    entityType = "MaintenanceReminder",
                    entityId = reminderId,
                    level,
                    levelName,
                    items
                }, JsonOptions);
                entityId = reminderId;
            }
            else
            {
                var vehicles = new[]
                {
                    new
                    {
                        userVehicleId = SeedUserVehicleId,
                        vehicleDisplayName = VehicleDisplayName,
                        licensePlate = VehicleDisplayName,
                        currentOdometer = 5000,
                        lastOdometerUpdateFormatted = DateTime.UtcNow.AddDays(-5).ToString("dd/MM/yyyy"),
                        daysSinceUpdate = 5
                    }
                };
                metadataJson = JsonSerializer.Serialize(new
                {
                    type = "OdometerReminder",
                    entityType = "OdometerReminder",
                    entityId = SeedUserVehicleId,
                    staleOdometerDays = 5,
                    vehicles
                }, JsonOptions);
                entityId = SeedUserVehicleId;
            }

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
                EntityId = entityId,
                MetadataJson = metadataJson,
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
