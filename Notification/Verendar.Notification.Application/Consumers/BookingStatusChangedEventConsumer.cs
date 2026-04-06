using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Application.Consumers;

public class BookingStatusChangedEventConsumer(
    ILogger<BookingStatusChangedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<BookingStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<BookingStatusChangedEvent> context)
    {
        var m = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing BookingStatusChanged — MessageId:{MessageId} BookingId:{BookingId} {From} → {To}",
            messageId, m.BookingId, m.FromStatus, m.ToStatus);

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingStatusChangedCopy(m);
            var actionUrl = routes.UserBookingHistoryUrl();

            var notification = NotificationMappings.CreateUserNotification(
                m.CustomerUserId,
                title,
                content,
                NotificationPriority.Medium,
                "Booking",
                m.BookingId,
                actionUrl);

            await ConsumerNotificationFlow.PersistWithInAppDeliveryAsync(
                unitOfWork, notification, m.CustomerUserId, context.CancellationToken);
            await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                inAppNotificationService,
                notificationService,
                m.CustomerUserId,
                title,
                content,
                notification.Id,
                context.CancellationToken);

            logger.LogInformation(
                "BookingStatusChanged processed — BookingId:{BookingId} NotificationId:{NotificationId}",
                m.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing BookingStatusChanged — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, m.BookingId);
        }
    }
}
