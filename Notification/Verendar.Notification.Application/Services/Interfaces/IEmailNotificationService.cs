using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface IEmailNotificationService
{
    Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default);
}
