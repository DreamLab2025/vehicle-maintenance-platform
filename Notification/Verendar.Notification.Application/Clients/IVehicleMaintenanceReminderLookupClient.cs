using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Notification.Application.Clients;

public interface IVehicleMaintenanceReminderLookupClient
{
    Task<IReadOnlyList<MaintenanceReminderLookupItemResponse>?> LookupAsync(
        Guid userId,
        IReadOnlyList<Guid> reminderIds,
        CancellationToken cancellationToken = default);
}
