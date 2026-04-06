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
        string? actionUrl,
        string? extendedPayloadJson = null)
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
            ActionUrl = actionUrl,
            ExtendedPayloadJson = extendedPayloadJson
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

    public static NotificationListItemDto ToSummaryListItemDto(this NotificationEntity n) =>
        ToListItemDtoCore(n, maintenanceItems: null);

    public static NotificationListItemDto ToListItemDto(this NotificationEntity n) =>
        ToListItemDtoCore(
            n,
            MaintenanceNotificationPayloadSerializer.TryDeserializeItems(n.ExtendedPayloadJson));

    private static NotificationListItemDto ToListItemDtoCore(
        NotificationEntity n,
        IReadOnlyList<MaintenanceNotificationItemDto>? maintenanceItems) =>
        new()
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
            MaintenanceItems = maintenanceItems
        };
}
