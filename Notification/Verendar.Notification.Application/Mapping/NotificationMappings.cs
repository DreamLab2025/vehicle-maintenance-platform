using System.Text.Json;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Enums;
using Verendar.Vehicle.Contracts.Events;


namespace Verendar.Notification.Application.Mapping
{
    public static class NotificationMappings
    {
        public static Domain.Entities.Notification OtpRequestedToNotificationEntity(
                this OtpRequestedEvent message,
                string title,
                string content,
                NotificationType type,
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
            NotificationType type,
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
                NotificationType = NotificationType.User,
                Priority = NotificationPriority.Medium,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new { Type = "OdometerReminder" }),
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

            var priority = message.Level switch
            {
                ReminderLevel.Low => NotificationPriority.Low,
                ReminderLevel.Medium => NotificationPriority.Medium,
                ReminderLevel.High => NotificationPriority.High,
                ReminderLevel.Critical => NotificationPriority.Critical,
                _ => NotificationPriority.Medium
            };

            return new Domain.Entities.Notification
            {
                UserId = message.UserId,
                Title = title,
                Message = content,
                NotificationType = NotificationType.User,
                Priority = priority,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    Type = "MaintenanceReminder",
                    Level = message.Level,
                    LevelName = NotificationConstants.MaintenanceLevelLabels.GetLabel(message.Level),
                    Items = message.Items
                }),
                EntityType = "MaintenanceReminder",
                EntityId = firstItem?.UserVehicleId
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

        public static NotificationListItemDto ToListItemDto(this Domain.Entities.Notification n)
        {
            return new NotificationListItemDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                Priority = n.Priority,
                Status = n.Status,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            };
        }

        public static NotificationDetailDto ToDetailDto(this Domain.Entities.Notification n)
        {
            return new NotificationDetailDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                Priority = n.Priority,
                Status = n.Status,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt,
                Metadata = string.IsNullOrEmpty(n.MetadataJson) ? null : JsonSerializer.Deserialize<JsonElement>(n.MetadataJson)
            };
        }
    }
}
