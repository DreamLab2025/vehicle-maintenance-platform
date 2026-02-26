using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Constants;
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
    private static readonly Guid[] SeedReminderIds =
    {
        Guid.Parse("f0000002-0000-0000-0000-000000000002"),
        Guid.Parse("f0000003-0000-0000-0000-000000000003"),
        Guid.Parse("f0000004-0000-0000-0000-000000000004"),
        Guid.Parse("f0000005-0000-0000-0000-000000000005")
    };
    private const string VehicleDisplayName = "59-TEST-01";

    // Align with Vehicle.MaintenanceReminderTestDataSeeder: single CurrentOdometer = 15000
    private const int SeedCurrentOdometer = 15000;
    private const int StaleOdometerDays = 5;

    // Match MaintenanceReminderMappings / InAppNotificationMappings
    private const string CriticalIntro = "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. "
        + "Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện (về mức bình thường).";
    private const string CtaUpdateApp = "\n\nVui lòng vào app cập nhật sau khi thay linh kiện để dừng nhắc nhở.";
    private const string NormalIntro = "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:";
    private const string PartLineFormat = "• {0} (số km hiện tại: {1:N0}, cần thay trước: {2:N0})";
    private const string OdometerMessageFormat = "Bạn đã không cập nhật số km (odo) trong {0} ngày qua. "
        + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";

    // Critical = 2 notifications (one per part), like EmailNotificationService.SendMaintenanceReminderAsync
    private static readonly (string Name, string Description, int CurrentOdo, int TargetOdo, decimal Pct)[] CriticalParts =
    {
        ("Dầu nhớt động cơ", "Dầu bôi trơn giúp làm mát và bảo vệ động cơ", SeedCurrentOdometer, 15250, 5m),
        ("Lọc dầu", "Loại bỏ tạp chất trong dầu động cơ", SeedCurrentOdometer, 15250, 5m)
    };

    // High / Medium: one notification each, like BuildContent (NormalIntro + partList)
    private static readonly (string Title, string PartName, string Description, int CurrentOdo, int TargetOdo, decimal Pct, int Level, string LevelName)[] NormalReminders =
    {
        ("Nhắc nhở bảo dưỡng (High)", "Lốp xe", "Đảm bảo độ bám đường và an toàn khi di chuyển", SeedCurrentOdometer, 20000, 25m, 3, "High"),
        ("Nhắc nhở bảo dưỡng (Medium)", "Má phanh", "Đảm bảo khả năng phanh an toàn", SeedCurrentOdometer, 18000, 30m, 2, "Medium")
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

        var reminderIdIndex = 0;
        var totalSeeded = 0;

        // Critical: 1 notification per part (same as EmailNotificationService + ToInAppPayloadForItem)
        foreach (var (name, description, currentOdo, targetOdo, pct) in CriticalParts)
        {
            var reminderId = SeedReminderIds[reminderIdIndex++];
            var title = $"{NotificationConstants.Titles.MaintenanceCriticalPart} {name}";
            var partLine = string.Format(PartLineFormat, name, currentOdo, targetOdo);
            var message = CriticalIntro + "\n\n" + partLine + CtaUpdateApp;

            var itemData = new
            {
                partCategoryName = name,
                description,
                userVehicleId = SeedUserVehicleId,
                reminderId,
                currentOdometer = currentOdo,
                targetOdometer = targetOdo,
                initialOdometer = currentOdo - 3000,
                percentageRemaining = pct,
                vehicleDisplayName = VehicleDisplayName,
                estimatedNextReplacementDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            var metadataJson = JsonSerializer.Serialize(new
            {
                type = "MaintenanceReminder",
                entityType = "MaintenanceReminder",
                entityId = reminderId,
                level = 4,
                levelName = "Critical",
                items = new[] { itemData }
            }, JsonOptions);

            await AddNotificationWithDeliveriesAsync(db, TestUserId, title, message, NotificationPriority.Critical,
                "MaintenanceReminder", SeedUserVehicleId, metadataJson, cancellationToken);
            totalSeeded++;
        }

        // High / Medium: 1 notification each (BuildContent + ToInAppPayload)
        foreach (var (title, partName, description, currentOdo, targetOdo, pct, level, levelName) in NormalReminders)
        {
            var reminderId = SeedReminderIds[reminderIdIndex++];
            var partLine = string.Format(PartLineFormat, partName, currentOdo, targetOdo);
            var message = NormalIntro + "\n\n" + partLine;

            var itemData = new
            {
                partCategoryName = partName,
                description,
                userVehicleId = SeedUserVehicleId,
                reminderId,
                currentOdometer = currentOdo,
                targetOdometer = targetOdo,
                initialOdometer = currentOdo - 3000,
                percentageRemaining = pct,
                vehicleDisplayName = VehicleDisplayName,
                estimatedNextReplacementDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            var metadataJson = JsonSerializer.Serialize(new
            {
                type = "MaintenanceReminder",
                entityType = "MaintenanceReminder",
                entityId = reminderId,
                level,
                levelName,
                items = new[] { itemData }
            }, JsonOptions);

            var priority = level == 3 ? NotificationPriority.High : NotificationPriority.Medium;
            await AddNotificationWithDeliveriesAsync(db, TestUserId, title, message, priority,
                "MaintenanceReminder", SeedUserVehicleId, metadataJson, cancellationToken);
            totalSeeded++;
        }

        // Odometer reminder (ToInAppPayload)
        var odometerTitle = NotificationConstants.Titles.OdometerReminder;
        var odometerMessage = string.Format(OdometerMessageFormat, StaleOdometerDays);
        var lastUpdate = DateTime.UtcNow.AddDays(-StaleOdometerDays);
        var vehiclesData = new[]
        {
            new
            {
                userVehicleId = SeedUserVehicleId,
                vehicleDisplayName = VehicleDisplayName,
                licensePlate = VehicleDisplayName,
                currentOdometer = SeedCurrentOdometer,
                lastOdometerUpdateFormatted = lastUpdate.ToString(NotificationConstants.DateFormats.DateOnly),
                daysSinceUpdate = StaleOdometerDays
            }
        };
        var odometerMetadataJson = JsonSerializer.Serialize(new
        {
            type = "OdometerReminder",
            entityType = "OdometerReminder",
            entityId = SeedUserVehicleId,
            staleOdometerDays = StaleOdometerDays,
            vehicles = vehiclesData
        }, JsonOptions);

        await AddNotificationWithDeliveriesAsync(db, TestUserId, odometerTitle, odometerMessage, NotificationPriority.Medium,
            "OdometerReminder", SeedUserVehicleId, odometerMetadataJson, cancellationToken);
        totalSeeded++;

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded {Count} notifications for test user (UserId: {UserId}) corresponding to user vehicle", totalSeeded, TestUserId);
    }

    private static async Task AddNotificationWithDeliveriesAsync(
        NotificationDbContext db,
        Guid userId,
        string title,
        string message,
        NotificationPriority priority,
        string entityType,
        Guid entityId,
        string metadataJson,
        CancellationToken cancellationToken)
    {
        var notification = new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = NotificationType.User,
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
            RecipientAddress = userId.ToString(),
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
        await Task.CompletedTask;
    }
}
