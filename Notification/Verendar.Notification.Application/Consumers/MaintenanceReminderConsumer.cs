using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class MaintenanceReminderConsumer(
    ILogger<MaintenanceReminderConsumer> logger,
    IEmailNotificationService emailNotificationService) : IConsumer<MaintenanceReminderEvent>
{
    private readonly ILogger<MaintenanceReminderConsumer> _logger = logger;
    private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;

    public async Task Consume(ConsumeContext<MaintenanceReminderEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogDebug("Processing MaintenanceReminder - MessageId: {MessageId}, UserId: {UserId}, Level: {Level}",
            messageId, message.UserId, message.LevelName);

        try
        {
            if (string.IsNullOrWhiteSpace(message.TargetValue))
            {
                _logger.LogWarning("MaintenanceReminderEvent has no TargetValue - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
                return;
            }

            var success = await _emailNotificationService.SendMaintenanceReminderEmailAsync(message, context.CancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "MaintenanceReminder email sent - MessageId: {MessageId}, UserId: {UserId}, Level: {Level}, Parts: {Count}",
                    messageId, message.UserId, message.LevelName, message.Items?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("MaintenanceReminder email send failed - MessageId: {MessageId}, UserId: {UserId}",
                    messageId, message.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MaintenanceReminder email - MessageId: {MessageId}, UserId: {UserId}",
                messageId, message.UserId);
            throw;
        }
    }
}
