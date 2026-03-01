using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class OdometerReminderConsumer(
        ILogger<OdometerReminderConsumer> logger,
        IEmailNotificationService emailNotificationService,
        IInAppNotificationService inAppNotificationService,
        IUnitOfWork unitOfWork) : IConsumer<OdometerReminderEvent>
    {
        private readonly ILogger<OdometerReminderConsumer> _logger = logger;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;
        private readonly IInAppNotificationService _inAppNotificationService = inAppNotificationService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Consume(ConsumeContext<OdometerReminderEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogDebug("Processing OdometerReminder - MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);

            try
            {
                var (emailSent, notificationId) = await _emailNotificationService.SendOdometerReminderAsync(message, context.CancellationToken);

                if (emailSent)
                    _logger.LogInformation("OdometerReminder email sent - MessageId: {MessageId}, UserId: {UserId}", messageId, message.UserId);
                else if (!string.IsNullOrWhiteSpace(message.TargetValue))
                    _logger.LogWarning("OdometerReminder email send failed - MessageId: {MessageId}, UserId: {UserId}", messageId, message.UserId);

                var inAppPayload = message.ToInAppPayload();
                await _inAppNotificationService.SendAsync(message.UserId, inAppPayload, context.CancellationToken);

                if (notificationId.HasValue)
                    await MarkInAppDeliverySentAndSaveMetadataAsync(notificationId.Value, message.UserId, inAppPayload.Metadata, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OdometerReminder - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                throw;
            }
        }

        private async Task MarkInAppDeliverySentAndSaveMetadataAsync(Guid notificationId, Guid userId, IReadOnlyDictionary<string, object?> metadata, CancellationToken cancellationToken)
        {
            var notification = await _unitOfWork.Notifications.FindOneAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.MetadataJson = JsonSerializer.Serialize(metadata);
                await _unitOfWork.Notifications.UpdateAsync(notification.Id, notification);
            }

            var delivery = await _unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                d.NotificationId == notificationId && d.Channel == NotificationChannel.InApp);
            if (delivery != null)
            {
                delivery.Status = NotificationStatus.Sent;
                delivery.SentAt = DateTime.UtcNow;
                delivery.DeliveredAt = DateTime.UtcNow;
                await _unitOfWork.NotificationDeliveries.UpdateAsync(delivery.Id, delivery);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
