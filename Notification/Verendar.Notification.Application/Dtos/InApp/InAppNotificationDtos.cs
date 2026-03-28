namespace Verendar.Notification.Application.Dtos.InApp;

public record InAppNotificationPayload
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
}

/// <summary>Thin in-app payloads (no metadata) for SignalR.</summary>
public static class InAppNotificationPayloadFactory
{
    private static readonly IReadOnlyDictionary<string, object?> Empty =
        new Dictionary<string, object?>();

    public static IReadOnlyDictionary<string, object?> EmptyMetadata => Empty;

    public static InAppNotificationPayload Thin(string title, string message) => new()
    {
        Title = title,
        Message = message,
        Metadata = Empty
    };
}
