using Verendar.Notification.Application.Mapping;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class OdometerReminderConsumer(
        ILogger<OdometerReminderConsumer> logger,
        IEmailNotificationService emailNotificationService,
        IInAppNotificationService inAppNotificationService,
        INotificationService notificationService) : IConsumer<OdometerReminderEvent>
    {
        private readonly ILogger<OdometerReminderConsumer> _logger = logger;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;
        private readonly IInAppNotificationService _inAppNotificationService = inAppNotificationService;
        private readonly INotificationService _notificationService = notificationService;

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
                    await _notificationService.MarkInAppDeliveredAsync(notificationId.Value, message.UserId, inAppPayload.Metadata, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OdometerReminder - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                throw;
            }
        }
    }
}
