using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;

namespace Verendar.Notification.Application.Consumers;

public class BookingCompletedEventConsumer(
    ILogger<BookingCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<BookingCompletedEvent>
{
    public async Task Consume(ConsumeContext<BookingCompletedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing BookingCompleted — MessageId:{MessageId} BookingId:{BookingId} UserId:{UserId}",
            messageId, message.BookingId, message.UserId);

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingCompletedCopy(message);
            var customerActionUrl = routes.UserBookingHistoryUrl();

            var notification = NotificationMappings.CreateUserNotification(
                message.UserId,
                title,
                content,
                NotificationPriority.High,
                "Booking",
                message.BookingId,
                customerActionUrl);

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
                "BookingCompleted notification sent — MessageId:{MessageId} BookingId:{BookingId} NotificationId:{NotificationId}",
                messageId, message.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing BookingCompleted — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, message.BookingId);
        }

        await NotifyStaffAsync(message, messageId, context.CancellationToken);
    }

    private async Task NotifyStaffAsync(BookingCompletedEvent message, string messageId, CancellationToken ct)
    {
        var (title, content) = GarageBookingNotificationMappings.BookingCompletedForStaffCopy(message);
        var staffActionUrl = appOptions.Value.GarageDashboardBookingsUrl(message.GarageId, message.GarageBranchId);

        if (message.OwnerUserId != Guid.Empty)
            await TrySendStaffInAppAsync(message.OwnerUserId, title, content, message, messageId, staffActionUrl, ct);

        foreach (var managerId in message.ManagerUserIds)
            await TrySendStaffInAppAsync(managerId, title, content, message, messageId, staffActionUrl, ct);
    }

    private async Task TrySendStaffInAppAsync(
        Guid staffUserId,
        string title,
        string content,
        BookingCompletedEvent message,
        string messageId,
        string actionUrl,
        CancellationToken ct)
    {
        try
        {
            var notification = NotificationMappings.CreateUserNotification(
                staffUserId,
                title,
                content,
                NotificationPriority.Medium,
                "Booking",
                message.BookingId,
                actionUrl);

            await ConsumerNotificationFlow.PersistWithInAppDeliveryAsync(
                unitOfWork, notification, staffUserId, ct);
            await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                inAppNotificationService,
                notificationService,
                staffUserId,
                title,
                content,
                notification.Id,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to notify staff {StaffUserId} for BookingCompleted — MessageId:{MessageId} BookingId:{BookingId}",
                staffUserId, messageId, message.BookingId);
        }
    }
}
