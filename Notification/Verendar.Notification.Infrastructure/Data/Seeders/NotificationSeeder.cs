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

    private static readonly NotificationSeedItem[] NotificationItems =
    {
        // Critical notification with multiple parts
        new NotificationSeedItem(
            Title: "Khẩn cấp: Cần thay linh kiện",
            Message: "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện.\n\n" +
                     "Các linh kiện cần chú ý:\n" +
                     "• Dầu nhớt động cơ (số km hiện tại: 5,000, cần thay trước: 6,000)\n" +
                     "• Lọc dầu (số km hiện tại: 5,000, cần thay trước: 6,000)",
            Type: NotificationType.User,
            Priority: NotificationPriority.Critical,
            EntityType: "MaintenanceReminder",
            Level: 4,
            LevelName: "Critical",
            Parts: new[]
            {
                ("Dầu nhớt động cơ", "Dầu bôi trơn giúp làm mát và bảo vệ động cơ", 5000, 6000, 5m),
                ("Lọc dầu", "Loại bỏ tạp chất trong dầu động cơ", 5000, 6000, 5m)
            }),

        // High notification
        new NotificationSeedItem(
            Title: "Nhắc nhở bảo dưỡng (High)",
            Message: "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:\n\n• Lốp xe (số km hiện tại: 15,000, cần thay trước: 20,000)",
            Type: NotificationType.User,
            Priority: NotificationPriority.High,
            EntityType: "MaintenanceReminder",
            Level: 3,
            LevelName: "High",
            Parts: new[] { ("Lốp xe", "Đảm bảo độ bám đường và an toàn khi di chuyển", 15000, 20000, 25m) }),

        // Medium notification
        new NotificationSeedItem(
            Title: "Nhắc nhở bảo dưỡng (Medium)",
            Message: "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:\n\n• Má phanh (số km hiện tại: 7,000, cần thay trước: 10,000)",
            Type: NotificationType.User,
            Priority: NotificationPriority.Medium,
            EntityType: "MaintenanceReminder",
            Level: 2,
            LevelName: "Medium",
            Parts: new[] { ("Má phanh", "Đảm bảo khả năng phanh an toàn", 7000, 10000, 30m) }),

        // Odometer reminder
        new NotificationSeedItem(
            Title: "Nhắc nhở cập nhật số km",
            Message: "Bạn đã không cập nhật số km (odo) trong 5 ngày qua. Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.",
            Type: NotificationType.User,
            Priority: NotificationPriority.Medium,
            EntityType: "OdometerReminder",
            Level: null,
            LevelName: null,
            Parts: Array.Empty<(string, string, int, int, decimal)>())
    };

    private record NotificationSeedItem(
        string Title,
        string Message,
        NotificationType Type,
        NotificationPriority Priority,
        string EntityType,
        int? Level,
        string? LevelName,
        (string Name, string Description, int CurrentOdo, int TargetOdo, decimal PctRemaining)[] Parts);

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

        var reminderIdIndex = 0;
        for (var i = 0; i < NotificationItems.Length; i++)
        {
            var item = NotificationItems[i];
            string metadataJson;
            Guid? entityId;

            if (item.EntityType == "MaintenanceReminder" && item.Level.HasValue && item.LevelName != null)
            {
                var items = item.Parts.Select(part =>
                {
                    var reminderId = SeedReminderIds[reminderIdIndex++];
                    return new
                    {
                        partCategoryName = part.Name,
                        description = part.Description,
                        userVehicleId = SeedUserVehicleId,
                        reminderId,
                        currentOdometer = part.CurrentOdo,
                        targetOdometer = part.TargetOdo,
                        initialOdometer = part.CurrentOdo - 3000,
                        percentageRemaining = part.PctRemaining,
                        vehicleDisplayName = VehicleDisplayName,
                        estimatedNextReplacementDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };
                }).ToArray();

                metadataJson = JsonSerializer.Serialize(new
                {
                    type = "MaintenanceReminder",
                    entityType = "MaintenanceReminder",
                    entityId = SeedUserVehicleId,
                    level = item.Level,
                    levelName = item.LevelName,
                    items
                }, JsonOptions);
                // Use UserVehicleId as EntityId for MaintenanceReminder (one notification can have multiple reminders)
                entityId = SeedUserVehicleId;
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
                Title = item.Title,
                Message = item.Message,
                NotificationType = item.Type,
                Priority = item.Priority,
                Status = NotificationStatus.Sent,
                EntityType = item.EntityType,
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
