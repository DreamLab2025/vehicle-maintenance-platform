using Verender.Identity.Contracts.Events;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface IEmailNotificationService
{
    Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default);

    Task<bool> SendOdometerReminderEmailAsync(OdometerReminderEvent message, CancellationToken cancellationToken = default);

    Task<bool> SendMaintenanceReminderEmailAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default);
}
