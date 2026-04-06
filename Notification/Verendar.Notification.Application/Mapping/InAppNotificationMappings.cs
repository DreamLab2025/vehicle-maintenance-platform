using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping
{
    public static class InAppNotificationMappings
    {
        private const string EntityTypeOdometerReminder = "OdometerReminder";
        private const string EntityTypeMaintenanceReminder = "MaintenanceReminder";
        private const string OdometerMessageIntro = "Bạn đã không cập nhật số km (odo) trong {0} ngày qua. "
            + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";
        private const string MaintenanceCriticalIntroInApp = "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. ";

        public static InAppNotificationPayload ToInAppPayload(this OdometerReminderEvent message)
        {
            var days = message.StaleOdometerDays > 0 ? message.StaleOdometerDays : NotificationConstants.Defaults.StaleOdometerDays;
            var messageContent = string.Format(OdometerMessageIntro, days);
            var firstVehicle = message.Vehicles?.FirstOrDefault();
            var vehiclesData = (message.Vehicles ?? []).Select(v => new Dictionary<string, object?>
            {
                ["userVehicleId"] = v.UserVehicleId,
                ["vehicleDisplayName"] = v.VehicleDisplayName,
                ["licensePlate"] = v.LicensePlate,
                ["currentOdometer"] = v.CurrentOdometer,
                ["lastOdometerUpdateFormatted"] = v.LastOdometerUpdate?.ToString(NotificationConstants.DateFormats.DateOnly),
                ["daysSinceUpdate"] = v.DaysSinceUpdate
            }).ToList();

            var metadata = new Dictionary<string, object?>
            {
                ["type"] = EntityTypeOdometerReminder,
                ["entityType"] = EntityTypeOdometerReminder,
                ["entityId"] = firstVehicle?.UserVehicleId,
                ["staleOdometerDays"] = days,
                ["vehicles"] = vehiclesData
            };

            return new InAppNotificationPayload
            {
                Title = NotificationConstants.Titles.OdometerReminder,
                Message = messageContent,
                Metadata = metadata
            };
        }

        public static InAppNotificationPayload ToInAppPayloadForItem(this MaintenanceReminderEvent message, MaintenanceReminderItemDto item)
        {
            var title = $"{NotificationConstants.Titles.MaintenanceCriticalPart} {item.PartCategoryName}";
            var partLine = string.Format("• {0} (số km hiện tại: {1:N0}, cần thay trước: {2:N0}). ", item.PartCategoryName, item.CurrentOdometer, item.TargetOdometer);
            var messageContent = MaintenanceCriticalIntroInApp + partLine + "Vui lòng vào app cập nhật sau khi thay linh kiện để dừng nhắc nhở.";
            var itemData = new Dictionary<string, object?>
            {
                ["partCategoryName"] = item.PartCategoryName,
                ["description"] = item.Description,
                ["userVehicleId"] = item.UserVehicleId,
                ["reminderId"] = item.ReminderId,
                ["currentOdometer"] = item.CurrentOdometer,
                ["targetOdometer"] = item.TargetOdometer,
                ["initialOdometer"] = item.InitialOdometer,
                ["percentageRemaining"] = item.PercentageRemaining,
                ["vehicleDisplayName"] = item.VehicleDisplayName,
                ["estimatedNextReplacementDate"] = item.EstimatedNextReplacementDate,
                ["level"] = (int)item.Level,
                ["levelName"] = item.Level.ToString()
            };
            var singleEffective = MaintenanceReminderMappings.AggregateReminderLevel(message.Level, [item]);
            var metadata = new Dictionary<string, object?>
            {
                ["type"] = EntityTypeMaintenanceReminder,
                ["entityType"] = EntityTypeMaintenanceReminder,
                ["entityId"] = item.ReminderId,
                ["level"] = (int)singleEffective,
                ["levelName"] = singleEffective.ToString(),
                ["items"] = new List<Dictionary<string, object?>> { itemData }
            };
            return new InAppNotificationPayload
            {
                Title = title,
                Message = messageContent,
                Metadata = metadata
            };
        }

        public static InAppNotificationPayload ToInAppPayload(this MaintenanceReminderEvent message)
        {
            var (title, messageContent) = message.BuildContent();
            var firstItem = message.Items?.FirstOrDefault();
            var list = message.Items ?? [];
            var bundleLevel = MaintenanceReminderMappings.AggregateReminderLevel(message.Level, list);
            var itemsData = list.Select(i => new Dictionary<string, object?>
            {
                ["partCategoryName"] = i.PartCategoryName,
                ["description"] = i.Description,
                ["userVehicleId"] = i.UserVehicleId,
                ["reminderId"] = i.ReminderId,
                ["currentOdometer"] = i.CurrentOdometer,
                ["targetOdometer"] = i.TargetOdometer,
                ["initialOdometer"] = i.InitialOdometer,
                ["percentageRemaining"] = i.PercentageRemaining,
                ["vehicleDisplayName"] = i.VehicleDisplayName,
                ["estimatedNextReplacementDate"] = i.EstimatedNextReplacementDate,
                ["level"] = (int)i.Level,
                ["levelName"] = i.Level.ToString()
            }).ToList();

            var metadata = new Dictionary<string, object?>
            {
                ["type"] = EntityTypeMaintenanceReminder,
                ["entityType"] = EntityTypeMaintenanceReminder,
                ["entityId"] = firstItem?.ReminderId,
                ["level"] = (int)bundleLevel,
                ["levelName"] = bundleLevel.ToString(),
                ["items"] = itemsData
            };

            return new InAppNotificationPayload
            {
                Title = title,
                Message = messageContent,
                Metadata = metadata
            };
        }

    }
}
