using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Application.Consumers;

internal static class ConsumerNotificationFlow
{
    public static async Task PersistWithInAppDeliveryAsync(
        IUnitOfWork unitOfWork,
        NotificationEntity notification,
        Guid inAppUserId,
        CancellationToken cancellationToken)
    {
        await unitOfWork.Notifications.AddAsync(notification);
        await unitOfWork.NotificationDeliveries.AddAsync(
            notification.CreateDelivery(inAppUserId.ToString(), NotificationChannel.InApp));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public static async Task FinalizeEmailDeliveryAsync(
        IUnitOfWork unitOfWork,
        NotificationDelivery delivery,
        bool success,
        CancellationToken cancellationToken)
    {
        delivery.Status = success ? NotificationStatus.Sent : NotificationStatus.Failed;
        if (success)
            delivery.SentAt = delivery.DeliveredAt = DateTime.UtcNow;
        await unitOfWork.NotificationDeliveries.UpdateAsync(delivery.Id, delivery);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public static async Task PushInAppAndMarkDeliveredAsync(
        IInAppNotificationService inApp,
        INotificationService notificationService,
        Guid userId,
        string title,
        string message,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        await inApp.SendAsync(userId, InAppNotificationPayloadFactory.Thin(title, message), cancellationToken);
        await notificationService.MarkInAppDeliveredAsync(
            notificationId, userId, InAppNotificationPayloadFactory.EmptyMetadata, cancellationToken);
    }
}
