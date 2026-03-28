using Verendar.Notification.Application.Dtos.Notifications;

namespace Verendar.Notification.Application.Mapping;

public static class NotificationMappings
{
    public static NotificationEntity CreateUserNotification(
        Guid userId,
        string title,
        string message,
        NotificationPriority priority,
        string entityType,
        Guid? entityId,
        string? actionUrl)
    {
        return new NotificationEntity
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = NotificationType.User,
            Priority = priority,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EntityType = entityType,
            EntityId = entityId,
            ActionUrl = actionUrl
        };
    }

    public static NotificationDelivery CreateDelivery(
        this NotificationEntity notification,
        string? recipientAddress,
        NotificationChannel channel)
    {
        return new NotificationDelivery
        {
            NotificationId = notification.Id,
            Channel = channel,
            RecipientAddress = recipientAddress,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static NotificationListItemDto ToListItemDto(this NotificationEntity n)
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

    public static NotificationDetailDto ToDetailDto(this NotificationEntity n)
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
            Metadata = null,
            MaintenanceReminderFreshness = null
        };
    }
}
