using System.Text.Json;
using System.Text.Json.Serialization;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class MaintenanceNotificationPayloadSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(MaintenanceNotificationPayload payload) =>
        JsonSerializer.Serialize(payload, JsonOptions);

    public static string SerializeFromEventItems(IEnumerable<MaintenanceReminderItemDto> items)
    {
        var payload = new MaintenanceNotificationPayload
        {
            Items = items.Select(i => new MaintenanceNotificationItemDto
            {
                PartCategoryName = i.PartCategoryName,
                Description = i.Description,
                CurrentOdometer = i.CurrentOdometer,
                TargetOdometer = i.TargetOdometer,
                PercentageRemaining = i.PercentageRemaining,
                EstimatedNextReplacementDate = i.EstimatedNextReplacementDate,
                Level = i.Level
            }).ToList()
        };
        return Serialize(payload);
    }

    public static IReadOnlyList<MaintenanceNotificationItemDto>? TryDeserializeItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var payload = JsonSerializer.Deserialize<MaintenanceNotificationPayload>(json, JsonOptions);
            if (payload?.Items is not { Count: > 0 } list)
                return null;
            return list;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
