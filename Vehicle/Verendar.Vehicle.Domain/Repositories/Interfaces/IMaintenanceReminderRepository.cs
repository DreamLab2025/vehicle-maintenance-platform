using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceReminderRepository : IGenericRepository<MaintenanceReminder>
    {
        Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceReminder>> GetPendingRemindersAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceReminder>> GetByLevelAsync(ReminderLevel level, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceReminder>> GetByLevelWithDetailsAsync(
            ReminderLevel level,
            bool includeAlreadyNotified,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdWithDetailsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
    }
}
