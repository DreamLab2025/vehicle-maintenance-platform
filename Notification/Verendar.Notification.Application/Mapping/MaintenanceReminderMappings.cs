using Verendar.Notification.Application.Constants;
using Verendar.Notification.Domain.Enums;
using Verendar.Vehicle.Contracts.Enums;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class MaintenanceReminderMappings
{
    private const string CriticalIntro = "Xe của bạn có linh kiện đã đến mức khẩn cấp cần thay thế. "
        + "Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện (về mức bình thường).";
    private const string NormalIntro = "Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế:";
    private const string PartListHeader = "Các linh kiện cần chú ý:";
    private const string PartListEmpty = "Các linh kiện cần bảo dưỡng/thay thế.";
    private const string CtaUpdateApp = "\n\nVui lòng vào app cập nhật sau khi thay linh kiện để dừng nhắc nhở.";
    private const string PartLineFormat = "• {0} (số km hiện tại: {1:N0}, cần thay trước: {2:N0})";

    public static NotificationPriority ToNotificationPriority(this ReminderLevel level) => level switch
    {
        ReminderLevel.Normal or ReminderLevel.Low => NotificationPriority.Low,
        ReminderLevel.Medium => NotificationPriority.Medium,
        ReminderLevel.High or ReminderLevel.Critical => NotificationPriority.High,
        _ => NotificationPriority.Medium
    };

    public static ReminderLevel AggregateReminderLevel(
        ReminderLevel envelopeLevel,
        IReadOnlyCollection<MaintenanceReminderItemDto> items)
    {
        if (items == null || items.Count == 0)
            return envelopeLevel;
        var maxItem = items.Max(i => (int)i.Level);
        return (ReminderLevel)Math.Max((int)envelopeLevel, maxItem);
    }

    public static (string Title, string Body) ToVehicleGroupCopy(
        ReminderLevel level,
        string vehicleDisplayName,
        int itemCount)
    {
        var title = level is ReminderLevel.Critical or ReminderLevel.High
            ? NotificationConstants.ConsumerCopy.MaintenanceTitleUrgent
            : NotificationConstants.ConsumerCopy.MaintenanceTitleNormal;
        var body = $"Xe {vehicleDisplayName} có {itemCount} hạng mục cần chú ý.";
        return (title, body);
    }

    public static (string Title, string Message) BuildSingleItemCriticalContent(this MaintenanceReminderItemDto item)
    {
        var title = $"{NotificationConstants.Titles.MaintenanceCriticalPart} {item.PartCategoryName}";
        var partLine = string.Format(PartLineFormat, item.PartCategoryName, item.CurrentOdometer, item.TargetOdometer);
        var body = CriticalIntro + "\n\n" + partLine + CtaUpdateApp;
        return (title, body);
    }

    public static (string Title, string Message) BuildContent(this MaintenanceReminderEvent message)
    {
        var items = message.Items ?? [];
        var effectiveLevel = AggregateReminderLevel(message.Level, items);
        var partList = items.Count > 0
            ? string.Join("\n", items.Select(i => string.Format(PartLineFormat, i.PartCategoryName, i.CurrentOdometer, i.TargetOdometer)))
            : PartListEmpty;

        if (effectiveLevel >= ReminderLevel.Critical)
        {
            var title = NotificationConstants.Titles.MaintenanceCritical;
            var body = CriticalIntro + "\n\n" + PartListHeader + "\n" + partList + CtaUpdateApp;
            return (title, body);
        }

        var partNames = items.Count > 0
            ? string.Join(", ", items.Select(i => i.PartCategoryName))
            : string.Empty;
        var levelLabel = NotificationConstants.MaintenanceLevelLabels.GetLabel(effectiveLevel);
        var normalTitle = string.IsNullOrEmpty(partNames)
            ? $"{levelLabel}: {NotificationConstants.Titles.MaintenanceNormalPrefix}"
            : $"{levelLabel}: {NotificationConstants.Titles.MaintenanceNormalPrefix} {partNames}";
        var normalBody = NormalIntro + "\n\n" + partList;
        return (normalTitle, normalBody);
    }

    public static MaintenanceReminderEvent ToSingleItemMessage(this MaintenanceReminderEvent message, MaintenanceReminderItemDto item)
    {
        return new MaintenanceReminderEvent
        {
            UserId = message.UserId,
            TargetValue = message.TargetValue,
            UserName = message.UserName,
            Level = AggregateReminderLevel(message.Level, [item]),
            Items = [item]
        };
    }

}
