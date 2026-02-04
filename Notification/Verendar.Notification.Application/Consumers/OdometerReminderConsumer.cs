using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class OdometerReminderConsumer(
    ILogger<OdometerReminderConsumer> logger,
    IEmailNotificationService emailNotificationService) : IConsumer<OdometerReminderEvent>
{
    private readonly ILogger<OdometerReminderConsumer> _logger = logger;
    private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;

    public async Task Consume(ConsumeContext<OdometerReminderEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogDebug("Processing OdometerReminder - MessageId: {MessageId}, UserId: {UserId}",
            messageId, message.UserId);

        try
        {
            if (string.IsNullOrWhiteSpace(message.TargetValue))
            {
                _logger.LogWarning("OdometerReminderEvent has no TargetValue - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                return;
            }

            var success = await _emailNotificationService.SendOdometerReminderEmailAsync(message, context.CancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "OdometerReminder email sent - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
            else
            {
                _logger.LogWarning("OdometerReminder email send failed - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OdometerReminder email - MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
            throw;
        }
    }
}
