using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceReminderEventPublisher
    {
        Task PublishAsync(MaintenanceReminderEvent message, CancellationToken cancellationToken = default);
    }
}
