using System.Text.Json;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Constants;
using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Enums;
using Verendar.Vehicle.Contracts.Events;


namespace Verendar.Notification.Application.Mapping
{
    public static class NotificationMappings
    {
        private static readonly JsonSerializerOptions MaintenanceReminderMetadataJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

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

        public static Domain.Entities.Notification MemberAccountCreatedToNotificationEntity(
            this MemberAccountCreatedEvent message,
            string title,
            string content)
        {
            return new Domain.Entities.Notification
            {
                UserId = message.UserId,
                Title = title,
                Message = content,
                NotificationType = NotificationType.System,
                Priority = NotificationPriority.High,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new { Role = message.Role })
            };
        }

        public static Domain.Entities.Notification BookingCreatedToNotificationEntity(
            this BookingCreatedEvent message,
            string title,
            string content)
        {
            return new Domain.Entities.Notification
            {
                UserId = message.UserId,
                Title = title,
                Message = content,
                NotificationType = NotificationType.User,
                Priority = NotificationPriority.Medium,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new { Type = "BookingCreated", BookingId = message.BookingId }),
                EntityType = "Booking",
                EntityId = message.BookingId
            };
        }

        public static Domain.Entities.Notification BookingCompletedToNotificationEntity(
            this BookingCompletedEvent message,
            string title,
            string content)
        {
            return new Domain.Entities.Notification
            {
                UserId = message.UserId,
                Title = title,
                Message = content,
                NotificationType = NotificationType.User,
                Priority = NotificationPriority.High,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    Type = "BookingCompleted",
                    BookingId = message.BookingId,
                    UserVehicleId = message.UserVehicleId
                }),
                EntityType = "MaintenanceProposal",
                EntityId = message.UserVehicleId
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
            var snapshotAtUtc = DateTime.UtcNow;

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
                CreatedAt = snapshotAtUtc,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    Type = "MaintenanceReminder",
                    Level = message.Level,
                    LevelName = NotificationConstants.MaintenanceLevelLabels.GetLabel(message.Level),
                    Items = message.Items,
                    SnapshotAtUtc = snapshotAtUtc
                }, MaintenanceReminderMetadataJsonOptions),
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
