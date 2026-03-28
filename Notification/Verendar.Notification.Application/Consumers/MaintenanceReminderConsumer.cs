using Microsoft.Extensions.Options;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Enums;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class MaintenanceReminderConsumer(
    ILogger<MaintenanceReminderConsumer> logger,
    IUnitOfWork unitOfWork,
    IEmailNotificationService emailNotificationService,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<MaintenanceReminderEvent>
{
    public async Task Consume(ConsumeContext<MaintenanceReminderEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing MaintenanceReminder — MessageId: {MessageId}, UserId: {UserId}, Level: {Level}",
            messageId, message.UserId, message.Level);

        var items = message.Items ?? [];
        if (items.Count == 0)
        {
            logger.LogWarning("MaintenanceReminder has no items — MessageId: {MessageId}", messageId);
            return;
        }

        var sendEmail = message.Level >= ReminderLevel.High
                        && !string.IsNullOrWhiteSpace(message.TargetValue);
        var groups = items.GroupBy(i => i.UserVehicleId).ToList();

        try
        {
            foreach (var group in groups)
            {
                var vehicleId = group.Key;
                var vehicleDisplayName = group.First().VehicleDisplayName
                    ?? NotificationConstants.ConsumerCopy.MaintenanceVehicleFallbackName;
                var count = group.Count();
                var (title, body) = MaintenanceReminderMappings.ToVehicleGroupCopy(
                    message.Level, vehicleDisplayName, count);
                var actionPath = routes.UserVehicleMaintenanceRelativeUrl(vehicleId);
                var actionAbsolute = routes.ToAbsoluteUrl(actionPath);

                var notification = NotificationMappings.CreateUserNotification(
                    message.UserId,
                    title,
                    body,
                    message.Level.ToNotificationPriority(),
                    "UserVehicle",
                    vehicleId,
                    actionPath);

                await unitOfWork.Notifications.AddAsync(notification);
                await unitOfWork.NotificationDeliveries.AddAsync(
                    notification.CreateDelivery(message.UserId.ToString(), NotificationChannel.InApp));

                NotificationDelivery? emailDelivery = null;
                if (sendEmail)
                {
                    emailDelivery = notification.CreateDelivery(message.TargetValue!, NotificationChannel.EMAIL);
                    await unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
                }

                await unitOfWork.SaveChangesAsync(context.CancellationToken);

                if (emailDelivery != null)
                {
                    var ok = await emailNotificationService.SendNotificationEmailAsync(
                        message.TargetValue!,
                        title,
                        body,
                        actionAbsolute,
                        NotificationConstants.ConsumerCopy.EmailCtaViewApp,
                        context.CancellationToken);
                    await ConsumerNotificationFlow.FinalizeEmailDeliveryAsync(
                        unitOfWork, emailDelivery, ok, context.CancellationToken);
                }

                await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                    inAppNotificationService,
                    notificationService,
                    message.UserId,
                    title,
                    body,
                    notification.Id,
                    context.CancellationToken);
            }

            logger.LogInformation(
                "MaintenanceReminder processed — MessageId: {MessageId}, UserId: {UserId}, Groups: {Count}",
                messageId, message.UserId, groups.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing MaintenanceReminder — MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
            throw;
        }
    }
}
