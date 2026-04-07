using Microsoft.Extensions.Options;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Options;

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
            var actionUrl = routes.UserProposalUrl(message.UserVehicleId);

            var notification = NotificationMappings.CreateUserNotification(
                message.UserId,
                title,
                content,
                NotificationPriority.Medium,
                "Booking",
                message.BookingId,
                actionUrl);

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

        await NotifyStaffAsync(message, messageId, actionUrl: routes.GarageDashboardBookingsUrl(message.GarageId, message.GarageBranchId), context.CancellationToken);
    }

    private async Task NotifyStaffAsync(BookingCreatedEvent message, string messageId, string actionUrl, CancellationToken ct)
    {
        var (title, content) = GarageBookingNotificationMappings.BookingCreatedForStaffCopy(message);

        // Owner always receives the notification (has visibility across all branches)
        if (message.OwnerUserId != Guid.Empty)
            await TrySendStaffInAppAsync(message.OwnerUserId, title, content, message, messageId, actionUrl, ct);

        // Each branch manager receives the notification independently
        foreach (var managerId in message.ManagerUserIds)
            await TrySendStaffInAppAsync(managerId, title, content, message, messageId, actionUrl, ct);

        if (message.OwnerUserId == Guid.Empty && message.ManagerUserIds.Count == 0)
            logger.LogWarning(
                "BookingCreated — no staff recipient found for branch {BranchId}, BookingId: {BookingId}",
                message.GarageBranchId, message.BookingId);
    }

    private async Task TrySendStaffInAppAsync(
        Guid staffUserId,
        string title,
        string content,
        BookingCreatedEvent message,
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
                NotificationPriority.High,
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
                "Failed to notify staff {StaffUserId} for BookingCreated — MessageId: {MessageId}, BookingId: {BookingId}",
                staffUserId, messageId, message.BookingId);
        }
    }
}
