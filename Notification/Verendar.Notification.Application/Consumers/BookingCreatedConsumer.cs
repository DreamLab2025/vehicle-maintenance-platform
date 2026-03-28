using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Application.Consumers;

public class BookingCreatedConsumer(
    ILogger<BookingCreatedConsumer> logger,
    IUnitOfWork unitOfWork,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<BookingCreatedEvent>
{
    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing BookingCreated — MessageId: {MessageId}, BookingId: {BookingId}, UserId: {UserId}",
            messageId, message.BookingId, message.UserId);

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingCreatedCopy(message);
            var actionPath = routes.BookingDetailRelativeUrl(message.BookingId);

            var notification = NotificationMappings.CreateUserNotification(
                message.UserId,
                title,
                content,
                NotificationPriority.Medium,
                "Booking",
                message.BookingId,
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
                "BookingCreated processed — MessageId: {MessageId}, BookingId: {BookingId}, NotificationId: {NotificationId}",
                messageId, message.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing BookingCreated — MessageId: {MessageId}, BookingId: {BookingId}",
                messageId, message.BookingId);
        }
    }
}
