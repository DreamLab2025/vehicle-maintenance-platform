using Microsoft.Extensions.Options;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class OdometerReminderConsumer(
    ILogger<OdometerReminderConsumer> logger,
    IUnitOfWork unitOfWork,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<OdometerReminderEvent>
{
    public async Task Consume(ConsumeContext<OdometerReminderEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug("Processing OdometerReminder — MessageId: {MessageId}, UserId: {UserId}",
            messageId, message.UserId);

        try
        {
            var days = message.StaleOdometerDays > 0
                ? message.StaleOdometerDays
                : NotificationConstants.Defaults.StaleOdometerDays;
            var title = NotificationConstants.Titles.OdometerReminder;
            var content =
                $"Bạn đã không cập nhật số km (odo) trong {days} ngày qua. "
                + "Vui lòng cập nhật số km của xe để Verendar có thể theo dõi bảo dưỡng chính xác hơn.";
            var firstVehicle = message.Vehicles?.FirstOrDefault();
            var entityId = firstVehicle?.UserVehicleId;
            var actionPath = entityId.HasValue
                ? routes.UserVehicleOdometerRelativeUrl(entityId.Value)
                : routes.UserVehiclesFallbackPath;

            var notification = NotificationMappings.CreateUserNotification(
                message.UserId,
                title,
                content,
                NotificationPriority.Medium,
                "OdometerReminder",
                entityId,
                actionPath);

            await ConsumerNotificationFlow.PersistWithInAppDeliveryAsync(
                unitOfWork, notification, message.UserId, context.CancellationToken);
            await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                inAppNotificationService,
                notificationService,
                message.UserId,
                title,
                content,
                notification.Id,
                context.CancellationToken);

            logger.LogInformation(
                "OdometerReminder processed — MessageId: {MessageId}, UserId: {UserId}, NotificationId: {NotificationId}",
                messageId, message.UserId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing OdometerReminder — MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
            throw;
        }
    }
}
