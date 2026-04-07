using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;

namespace Verendar.Notification.Application.Consumers;

public class BookingConfirmedEventConsumer(
    ILogger<BookingConfirmedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IEmailNotificationService emailNotificationService,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService,
    IOptions<NotificationAppOptions> appOptions) : IConsumer<BookingConfirmedEvent>
{
    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var m = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var routes = appOptions.Value;

        logger.LogDebug(
            "Processing BookingConfirmed — MessageId:{MessageId} BookingId:{BookingId} Customer:{Customer}",
            messageId, m.BookingId, m.CustomerUserId);

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingConfirmedCopy(m);
            var customerActionUrl = routes.UserProposalUrl(m.UserVehicleId);

            var notification = NotificationMappings.CreateUserNotification(
                m.CustomerUserId,
                title,
                content,
                NotificationPriority.High,
                "Booking",
                m.BookingId,
                customerActionUrl);

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
                    customerActionUrl,
                    NotificationConstants.ConsumerCopy.EmailCtaViewBooking,
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
                "BookingConfirmed processed — BookingId:{BookingId} NotificationId:{NotificationId}",
                m.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing BookingConfirmed — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, m.BookingId);
        }

        await NotifyMechanicAsync(m, messageId, context.CancellationToken);
    }

    private async Task NotifyMechanicAsync(BookingConfirmedEvent m, string messageId, CancellationToken ct)
    {
        if (m.MechanicUserId == Guid.Empty)
        {
            logger.LogWarning(
                "BookingConfirmed — MechanicUserId missing, skipping mechanic notification. BookingId:{BookingId}",
                m.BookingId);
            return;
        }

        try
        {
            var (title, content) = GarageBookingNotificationMappings.BookingAssignedToMechanicCopy(m);
            var mechanicActionUrl = appOptions.Value.GarageDashboardRequiresUrl(m.GarageId, m.GarageBranchId);

            var notification = NotificationMappings.CreateUserNotification(
                m.MechanicUserId,
                title,
                content,
                NotificationPriority.High,
                "Booking",
                m.BookingId,
                mechanicActionUrl);

            await ConsumerNotificationFlow.PersistWithInAppDeliveryAsync(
                unitOfWork, notification, m.MechanicUserId, ct);
            await ConsumerNotificationFlow.PushInAppAndMarkDeliveredAsync(
                inAppNotificationService,
                notificationService,
                m.MechanicUserId,
                title,
                content,
                notification.Id,
                ct);

            logger.LogInformation(
                "BookingConfirmed mechanic notified — BookingId:{BookingId} MechanicUserId:{MechanicUserId}",
                m.BookingId, m.MechanicUserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to notify mechanic for BookingConfirmed — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, m.BookingId);
        }
    }
}
