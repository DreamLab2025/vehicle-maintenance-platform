using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Vehicle.Contracts.Enums;
using NotificationEntity = Verendar.Notification.Domain.Entities.Notification;

namespace Verendar.Notification.Infrastructure.Seeders;

public static class NotificationSeeder
{
    private static readonly Guid SystemUserId = Guid.Empty;
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string TestUserEmail = "hoalvpse181951@fpt.edu.vn";

    private static readonly Guid SeedUserVehicleId = Guid.Parse("f0000001-0000-0000-0000-000000000001");

    private static string MaintenanceActionPath(Guid userVehicleId) =>
        $"/user-vehicles/{userVehicleId}/maintenance-records";

    private static string OdometerActionPath(Guid userVehicleId) =>
        $"/user-vehicles/{userVehicleId}/odometer";

    private const int StaleOdometerDays = 5;

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
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.NotificationPreferences.Add(preference);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded notification preference for test user (UserId: {UserId}, Email: {Email})", TestUserId, TestUserEmail);
    }

    private static async Task SeedNotificationsAsync(NotificationDbContext db, ILogger? logger, CancellationToken cancellationToken)
    {
        var vehicleRelatedTypes = new[] { "UserVehicle", "MaintenanceReminder", "OdometerReminder" };
        var existingCount = await db.Notifications
            .IgnoreQueryFilters()
            .CountAsync(n =>
                    n.UserId == TestUserId
                    && n.EntityId == SeedUserVehicleId
                    && n.EntityType != null
                    && vehicleRelatedTypes.Contains(n.EntityType),
                cancellationToken);
        if (existingCount > 0)
        {
            logger?.LogDebug(
                "Test user already has {Count} vehicle-related notifications (UserId: {UserId}), skip seed",
                existingCount,
                TestUserId);
            return;
        }

        var vehicleDisplayName = "Wave Alpha - 59-TEST-01";
        var totalSeeded = 0;

        var maintenanceSamples = new (ReminderLevel Level, int ItemCount)[]
        {
            (ReminderLevel.Critical, 2),
            (ReminderLevel.High, 1),
            (ReminderLevel.Medium, 2),
            (ReminderLevel.Medium, 1)
        };

        foreach (var (level, itemCount) in maintenanceSamples)
        {
            var (title, body) = MaintenanceReminderMappings.ToVehicleGroupCopy(level, vehicleDisplayName, itemCount);
            var payloadJson = MaintenanceNotificationPayloadSerializer.Serialize(
                BuildSampleMaintenancePayload(itemCount, level));
            AddSentInAppNotification(
                db,
                TestUserId,
                title,
                body,
                level.ToNotificationPriority(),
                "UserVehicle",
                SeedUserVehicleId,
                MaintenanceActionPath(SeedUserVehicleId),
                payloadJson);
            totalSeeded++;
        }

        var odometerTitle = NotificationConstants.Titles.OdometerReminder;
        var odometerMessage =
            $"Bạn đã không cập nhật số km (odo) trong {StaleOdometerDays} ngày qua. "
            + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";
        AddSentInAppNotification(
            db,
            TestUserId,
            odometerTitle,
            odometerMessage,
            NotificationPriority.Medium,
            "OdometerReminder",
            SeedUserVehicleId,
            OdometerActionPath(SeedUserVehicleId));
        totalSeeded++;

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation(
            "Seeded {Count} thin notifications for test user (UserId: {UserId}, UserVehicleId: {VehicleId})",
            totalSeeded,
            TestUserId,
            SeedUserVehicleId);
    }

    private static MaintenanceNotificationPayload BuildSampleMaintenancePayload(int itemCount, ReminderLevel level)
    {
        string[] names = ["Dầu máy", "Lốp trước", "Phanh", "Ắc quy", "Dây curoa"];
        var items = Enumerable.Range(0, itemCount).Select(i => new MaintenanceNotificationItemDto
        {
            PartCategoryName = i < names.Length ? names[i] : $"Hạng mục mẫu {i + 1}",
            Description = "Dữ liệu seed — ví dụ hạng mục bảo dưỡng",
            CurrentOdometer = 12_000 + i * 500,
            TargetOdometer = 15_000,
            PercentageRemaining = Math.Max(0, 25m - i * 8m),
            EstimatedNextReplacementDate = DateTime.UtcNow.AddDays(14 + i * 7),
            Level = level
        }).ToList();
        return new MaintenanceNotificationPayload { Items = items };
    }

    private static void AddSentInAppNotification(
        NotificationDbContext db,
        Guid userId,
        string title,
        string message,
        NotificationPriority priority,
        string entityType,
        Guid entityId,
        string actionUrl,
        string? extendedPayloadJson = null)
    {
        var notification = NotificationMappings.CreateUserNotification(
            userId,
            title,
            message,
            priority,
            entityType,
            entityId,
            actionUrl,
            extendedPayloadJson);
        notification.CreatedBy = SystemUserId;
        notification.Status = NotificationStatus.Sent;
        db.Notifications.Add(notification);

        var inApp = notification.CreateDelivery(userId.ToString(), NotificationChannel.InApp);
        inApp.CreatedBy = SystemUserId;
        inApp.Status = NotificationStatus.Sent;
        inApp.SentAt = DateTime.UtcNow;
        inApp.DeliveredAt = DateTime.UtcNow;
        db.NotificationDeliveries.Add(inApp);
    }
}
