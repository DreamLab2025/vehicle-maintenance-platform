using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Vehicle.Application.Services.Interfaces;

public interface IMaintenanceReminderLookupService
{
    Task<ApiResponse<IReadOnlyList<MaintenanceReminderLookupItemResponse>>> LookupForNotificationAsync(
        MaintenanceReminderLookupRequest request,
        CancellationToken cancellationToken = default);
}
