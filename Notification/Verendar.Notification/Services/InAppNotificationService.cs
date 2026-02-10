using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Hubs;

namespace Verendar.Notification.Services
{
    public class InAppNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<InAppNotificationService> logger) : IInAppNotificationService
    {
        public const string MethodName = "Notification";

        public async Task SendAsync(Guid userId, InAppNotificationPayload payload, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.Clients.User(userId.ToString()).SendAsync(MethodName, payload, cancellationToken);
                logger.LogDebug("Sent in-app notification to user {UserId}, type: {Type}", userId,
                    payload.Metadata.TryGetValue("type", out var t) ? t : null);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send in-app notification to user {UserId}", userId);
            }
        }
    }
}
