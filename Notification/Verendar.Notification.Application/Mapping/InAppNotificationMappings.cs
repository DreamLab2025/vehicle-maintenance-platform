using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class InAppNotificationMappings
{
    private const string OdometerReminderTitle = "Nhắc nhở cập nhật số km";
    private const int UrgentLevel = 4;

    public static InAppNotificationPayload ToInAppPayload(this OdometerReminderEvent message)
    {
        var days = message.StaleOdometerDays > 0 ? message.StaleOdometerDays : 3;
        var messageContent = $"Bạn đã không cập nhật số km (odo) trong {days} ngày qua. "
            + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";

        var firstVehicle = message.Vehicles?.FirstOrDefault();
        var vehiclesData = (message.Vehicles ?? []).Select(v => new Dictionary<string, object?>
        {
            ["userVehicleId"] = v.UserVehicleId,
            ["vehicleDisplayName"] = v.VehicleDisplayName,
            ["licensePlate"] = v.LicensePlate,
            ["currentOdometer"] = v.CurrentOdometer,
            ["lastOdometerUpdateFormatted"] = v.LastOdometerUpdate?.ToString("dd/MM/yyyy"),
            ["daysSinceUpdate"] = v.DaysSinceUpdate
        }).ToList();

        var metadata = new Dictionary<string, object?>
        {
            ["type"] = "OdometerReminder",
            ["entityType"] = "OdometerReminder",
            ["entityId"] = firstVehicle?.UserVehicleId,
            ["staleOdometerDays"] = days,
            ["vehicles"] = vehiclesData
        };

        return new InAppNotificationPayload
        {
            Title = OdometerReminderTitle,
            Message = messageContent,
            Metadata = metadata
        };
    }

    public static InAppNotificationPayload ToInAppPayload(this MaintenanceReminderEvent message)
    {
        var (title, messageContent) = BuildMaintenanceReminderContent(message);
        var firstItem = message.Items?.FirstOrDefault();
        var itemsData = (message.Items ?? []).Select(i => new Dictionary<string, object?>
        {
            ["partCategoryName"] = i.PartCategoryName,
            ["userVehicleId"] = i.UserVehicleId,
            ["reminderId"] = i.ReminderId,
            ["currentOdometer"] = i.CurrentOdometer,
            ["targetOdometer"] = i.TargetOdometer,
            ["percentageRemaining"] = i.PercentageRemaining,
            ["vehicleDisplayName"] = i.VehicleDisplayName
        }).ToList();

        var metadata = new Dictionary<string, object?>
        {
            ["type"] = "MaintenanceReminder",
            ["entityType"] = "MaintenanceReminder",
            ["entityId"] = firstItem?.ReminderId,
            ["level"] = message.Level,
            ["levelName"] = message.LevelName ?? string.Empty,
            ["items"] = itemsData
        };

        return new InAppNotificationPayload
        {
            Title = title,
            Message = messageContent,
            Metadata = metadata
        };
    }

    private static (string Title, string Message) BuildMaintenanceReminderContent(MaintenanceReminderEvent message)
    {
        var partList = (message.Items?.Count ?? 0) > 0
            ? string.Join("\n", (message.Items ?? []).Select(i => $"• {i.PartCategoryName} (số km hiện tại: {i.CurrentOdometer:N0}, cần thay trước: {i.TargetOdometer:N0})"))
            : "Các linh kiện cần bảo dưỡng/thay thế.";

        if (message.Level >= UrgentLevel)
        {
            var title = "Khẩn cấp: Cần thay linh kiện";
            var body = "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. "
                + "Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện (về mức bình thường).\n\n"
                + "Các linh kiện cần chú ý:\n" + partList
                + "\n\nVui lòng vào app cập nhật sau khi thay linh kiện để dừng nhắc nhở.";
            return (title, body);
        }

        var normalTitle = $"Nhắc nhở bảo dưỡng ({message.LevelName})";
        var normalBody = "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:\n\n" + partList;
        return (normalTitle, normalBody);
    }
}
