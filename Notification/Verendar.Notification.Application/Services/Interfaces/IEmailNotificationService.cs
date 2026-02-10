using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface IEmailNotificationService
    {
        Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default);

        Task<(bool EmailSent, Guid? NotificationId)> SendOdometerReminderAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default);

        Task<(bool EmailSent, Guid? NotificationId)> SendMaintenanceReminderAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default);
    }
}
