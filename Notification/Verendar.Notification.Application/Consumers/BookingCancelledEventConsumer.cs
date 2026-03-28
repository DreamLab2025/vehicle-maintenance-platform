using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Application.Consumers;

public class BookingCancelledEventConsumer(
    ILogger<BookingCancelledEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IEmailNotificationService emailNotificationService,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<BookingCancelledEvent>
{
    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var m = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing BookingCancelled — MessageId:{MessageId} BookingId:{BookingId} Customer:{Customer}",
            messageId, m.BookingId, m.CustomerUserId);

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingCancelledCopy(m);
            var actionPath = routes.BookingDetailRelativeUrl(m.BookingId);
            var actionAbsolute = routes.ToAbsoluteUrl(actionPath);

            var notification = NotificationMappings.CreateUserNotification(
                m.CustomerUserId,
                title,
                content,
                NotificationPriority.High,
                "Booking",
                m.BookingId,
                actionPath);

            await unitOfWork.Notifications.AddAsync(notification);
            await unitOfWork.NotificationDeliveries.AddAsync(
                notification.CreateDelivery(m.CustomerUserId.ToString(), NotificationChannel.InApp));

            NotificationDelivery? emailDelivery = null;
            if (!string.IsNullOrWhiteSpace(m.CustomerEmail))
            {
                emailDelivery = notification.CreateDelivery(m.CustomerEmail, NotificationChannel.EMAIL);
                await unitOfWork.NotificationDeliveries.AddAsync(emailDelivery);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            if (emailDelivery != null && !string.IsNullOrWhiteSpace(m.CustomerEmail))
            {
                var ok = await emailNotificationService.SendNotificationEmailAsync(
                    m.CustomerEmail,
                    title,
                    content,
                    actionAbsolute,
                    NotificationConstants.ConsumerCopy.EmailCtaViewDetail,
                    null,
                    context.CancellationToken);
                await ConsumerNotificationFlow.FinalizeEmailDeliveryAsync(
                    unitOfWork, emailDelivery, ok, context.CancellationToken);
            }

            await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                inAppNotificationService,
                notificationService,
                m.CustomerUserId,
                title,
                content,
                notification.Id,
                context.CancellationToken);

            logger.LogInformation(
                "BookingCancelled processed — BookingId:{BookingId} NotificationId:{NotificationId}",
                m.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing BookingCancelled — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, m.BookingId);
        }
    }
}
