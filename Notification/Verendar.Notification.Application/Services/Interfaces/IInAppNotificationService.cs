using Verendar.Notification.Application.Dtos.InApp;

namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface IInAppNotificationService
    {
        Task SendAsync(Guid userId, InAppNotificationPayload payload, CancellationToken cancellationToken = default);
    }
}
