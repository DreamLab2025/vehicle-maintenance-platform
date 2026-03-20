using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class MaintenanceReminderConsumer(
        ILogger<MaintenanceReminderConsumer> logger,
        IEmailNotificationService emailNotificationService,
        IInAppNotificationService inAppNotificationService,
        INotificationService notificationService) : IConsumer<MaintenanceReminderEvent>
    {
        private readonly ILogger<MaintenanceReminderConsumer> _logger = logger;
        private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;
        private readonly IInAppNotificationService _inAppNotificationService = inAppNotificationService;
        private readonly INotificationService _notificationService = notificationService;

        public async Task Consume(ConsumeContext<MaintenanceReminderEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogDebug("Processing MaintenanceReminder - MessageId: {MessageId}, UserId: {UserId}, Level: {Level}",
                messageId, message.UserId, message.LevelName);

            try
            {
                var (emailSent, notificationIds) = await _emailNotificationService.SendMaintenanceReminderAsync(message, context.CancellationToken);
                var idsList = notificationIds.ToList();

                if (emailSent)
                    _logger.LogInformation(
                        "MaintenanceReminder email sent - MessageId: {MessageId}, UserId: {UserId}, Level: {Level}, Notifications: {Count}",
                        messageId, message.UserId, message.LevelName, idsList.Count);
                else if (!string.IsNullOrWhiteSpace(message.TargetValue))
                    _logger.LogWarning("MaintenanceReminder email send failed - MessageId: {MessageId}, UserId: {UserId}", messageId, message.UserId);

                var items = message.Items ?? [];
                if (idsList.Count == items.Count && items.Count > 0)
                {
                    for (var i = 0; i < idsList.Count; i++)
                    {
                        var payload = message.ToInAppPayloadForItem(items[i]);
                        await _inAppNotificationService.SendAsync(message.UserId, payload, context.CancellationToken);
                        await _notificationService.MarkInAppDeliveredAsync(idsList[i], message.UserId, payload.Metadata, context.CancellationToken);
                    }
                }
                else
                {
                    var inAppPayload = message.ToInAppPayload();
                    await _inAppNotificationService.SendAsync(message.UserId, inAppPayload, context.CancellationToken);
                    if (idsList.Count > 0)
                        await _notificationService.MarkInAppDeliveredAsync(idsList[0], message.UserId, inAppPayload.Metadata, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MaintenanceReminder - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                throw;
            }
        }
    }
}
