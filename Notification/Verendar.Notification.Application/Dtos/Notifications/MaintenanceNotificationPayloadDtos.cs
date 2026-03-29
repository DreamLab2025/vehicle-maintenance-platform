namespace Verendar.Notification.Application.Dtos.Notifications;

public record MaintenanceNotificationPayload
{
    public List<MaintenanceNotificationItemDto> Items { get; init; } = [];
}

public record MaintenanceNotificationItemDto
{
    public string PartCategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CurrentOdometer { get; init; }
    public int TargetOdometer { get; init; }
    public decimal PercentageRemaining { get; init; }
    public DateTime? EstimatedNextReplacementDate { get; init; }
}
