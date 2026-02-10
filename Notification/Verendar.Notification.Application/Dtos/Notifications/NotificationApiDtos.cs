using System.Text.Json;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Application.Dtos.Notifications
{
    public record NotificationListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public NotificationType NotificationType { get; init; }
        public NotificationPriority Priority { get; init; }
        public NotificationStatus Status { get; init; }
        public string? EntityType { get; init; }
        public Guid? EntityId { get; init; }
        public string? ActionUrl { get; init; }
        public bool IsRead { get; init; }
        public DateTime? ReadAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record NotificationDetailDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public NotificationType NotificationType { get; init; }
        public NotificationPriority Priority { get; init; }
        public NotificationStatus Status { get; init; }
        public string? EntityType { get; init; }
        public Guid? EntityId { get; init; }
        public string? ActionUrl { get; init; }
        public bool IsRead { get; init; }
        public DateTime? ReadAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public JsonElement? Metadata { get; init; }
    }

    public record NotificationStatusDto
    {
        public int UnReadCount { get; init; }
        public bool HasUnread => UnReadCount > 0;
    }
}
