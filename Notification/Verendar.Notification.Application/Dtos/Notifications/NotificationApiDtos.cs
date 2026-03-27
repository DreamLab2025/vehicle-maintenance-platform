using System.Text.Json;
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
        public DateTime CreatedAt { get; init; }
        public JsonElement? Metadata { get; init; }
        public IReadOnlyList<MaintenanceReminderFreshnessDto>? MaintenanceReminderFreshness { get; init; }
    }

    public record MaintenanceReminderFreshnessDto
    {
        public Guid ReminderId { get; init; }
        public Guid PartTrackingId { get; init; }
        public bool ReminderIsActive { get; init; }
        public string ReminderStatus { get; init; } = string.Empty;
        public DateOnly? LastReplacementDate { get; init; }
        public int? LastReplacementOdometer { get; init; }

        /// <summary>True khi reminder không còn Active hoặc ngày thay phụ tùng &gt;= ngày tạo thông báo (UTC).</summary>
        public bool TrackingChangedSinceNotification { get; init; }
    }

    public record NotificationStatusDto
    {
        public int UnReadCount { get; init; }
        public bool HasUnread => UnReadCount > 0;
    }
}
