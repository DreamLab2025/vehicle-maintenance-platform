using Verendar.Notification.Application.Dtos.Notifications;
using NotificationEntity = Verendar.Notification.Domain.Entities.Notification;

namespace Verendar.Notification.Application.Mapping;

public static class NotificationApiMappings
{
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
            ExpiresAt = n.ExpiresAt,
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
            ExpiresAt = n.ExpiresAt,
            CreatedAt = n.CreatedAt,
            MetadataJson = n.MetadataJson
        };
    }
}
