namespace Verendar.Notification.Application.Dtos.InApp;

public record InAppNotificationPayload
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
}
