using Microsoft.AspNetCore.SignalR;
using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Notification.Application.Hubs;

namespace Verendar.Notification.Application.Services.Implements
{
    public class InAppNotificationService(IHubContext<NotificationHub> hubContext) : IInAppNotificationService
    {
        public const string MethodName = "Notification";

        public Task SendAsync(Guid userId, InAppNotificationPayload payload, CancellationToken cancellationToken = default) =>
            hubContext.Clients.User(userId.ToString()).SendAsync(MethodName, payload, cancellationToken);
    }
}
