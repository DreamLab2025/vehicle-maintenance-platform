using Microsoft.AspNetCore.SignalR;
using Verendar.Notification.Application.Hubs;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers
{
    public class ForceTokenRefreshConsumer(
        ILogger<ForceTokenRefreshConsumer> logger,
        IHubContext<NotificationHub> hubContext) : IConsumer<ForceTokenRefreshEvent>
    {
        private readonly ILogger<ForceTokenRefreshConsumer> _logger = logger;
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;

        public async Task Consume(ConsumeContext<ForceTokenRefreshEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Processing ForceTokenRefreshEvent - MessageId: {MessageId}, UserId: {UserId}, Reason: {Reason}",
                messageId, message.UserId, message.Reason);

            try
            {
                await _hubContext.Clients
                    .User(message.UserId.ToString())
                    .SendAsync("ForceTokenRefresh", new { reason = message.Reason },
                        context.CancellationToken);

                _logger.LogInformation(
                    "Sent ForceTokenRefresh signal to user {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending ForceTokenRefresh to user {UserId} - MessageId: {MessageId}",
                    message.UserId, messageId);
            }
        }
    }
}
