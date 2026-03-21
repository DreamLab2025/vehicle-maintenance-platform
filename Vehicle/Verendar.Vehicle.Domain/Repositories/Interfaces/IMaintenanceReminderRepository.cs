using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceReminderRepository : IGenericRepository<MaintenanceReminder>
    {
        /// <summary>Toàn bộ reminder (mọi status) của xe — dùng cho lịch sử.</summary>
        Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        /// <summary>Reminder Active chưa notify, chưa dismiss — dùng cho background job nhẹ.</summary>
        Task<IEnumerable<MaintenanceReminder>> GetPendingRemindersAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        /// <summary>Reminder Active theo level.</summary>
        Task<IEnumerable<MaintenanceReminder>> GetByLevelAsync(ReminderLevel level, CancellationToken cancellationToken = default);
        /// <summary>Reminder Active theo level, kèm full navigation (cho event publish).</summary>
        Task<IEnumerable<MaintenanceReminder>> GetByLevelWithDetailsAsync(
            ReminderLevel level,
            bool includeAlreadyNotified,
            CancellationToken cancellationToken = default);
        /// <summary>Reminder Active của xe, kèm full navigation (cho publish immediate).</summary>
        Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdWithDetailsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
    }
}
