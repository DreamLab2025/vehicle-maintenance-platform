using System.Text.Json;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class NotificationMappings
{
    public static Domain.Entities.Notification OtpRequestedToNotificationEntity(
            this OtpRequestedEvent message,
            string title,
            string content,
            Domain.Enums.NotificationType type,
            bool isFallback = false)
    {
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = type,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new
            {
                OtpType = message.Type,
                Expiry = message.ExpiryTime,
                IsFallback = isFallback,
            })
        };
    }

    public static Domain.Entities.Notification UserRegisteredToNotificationEntity(
        this UserRegisteredEvent message,
        string title,
        string content,
        Domain.Enums.NotificationType type,
        bool isFallback = false)
    {
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = type,
            Priority = isFallback ? NotificationPriority.Low : NotificationPriority.Medium,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new
            {
                IsFallback = isFallback,
            })
        };
    }

    public static Domain.Entities.Notification OdometerReminderToNotificationEntity(
        this OdometerReminderEvent message,
        string title,
        string content)
    {
        var firstVehicle = message.Vehicles?.FirstOrDefault();
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = NotificationType.OdometerReminder,
            Priority = NotificationPriority.Medium,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new { Source = "OdometerReminder" }),
            EntityType = "OdometerReminder",
            EntityId = firstVehicle?.UserVehicleId
        };
    }

    public static Domain.Entities.Notification MaintenanceReminderToNotificationEntity(
        this MaintenanceReminderEvent message,
        string title,
        string content)
    {
        var firstItem = message.Items?.FirstOrDefault();
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = NotificationType.MaintenanceReminder,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new { Source = "MaintenanceReminder", Level = message.Level, LevelName = message.LevelName }),
            EntityType = "MaintenanceReminder",
            EntityId = firstItem?.ReminderId
        };
    }

    public static NotificationDelivery CreateDelivery(
        this Domain.Entities.Notification notification,
        string? recipientAddress,
        NotificationChannel channel)
    {
        return new NotificationDelivery
        {
            NotificationId = notification.Id,
            Channel = channel,
            RecipientAddress = recipientAddress,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MaxRetries = 3
        };
    }
}
