using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceReminderRepository(VehicleDbContext context) : PostgresRepository<MaintenanceReminder>(context), IMaintenanceReminderRepository
    {
        public async Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt.PartCategory)
                .Where(x => x.TrackingCycle.PartTracking.UserVehicleId == userVehicleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetPendingRemindersAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt.PartCategory)
                .Where(x =>
                    x.TrackingCycle.PartTracking.UserVehicleId == userVehicleId &&
                    x.Status == ReminderStatus.Active &&
                    !x.IsNotified &&
                    !x.IsDismissed)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByLevelAsync(ReminderLevel level, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt.PartCategory)
                .Where(x => x.Level == level && x.Status == ReminderStatus.Active && !x.IsNotified)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByLevelWithDetailsAsync(
            ReminderLevel level,
            bool includeAlreadyNotified,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt!.UserVehicle)
                            .ThenInclude(uv => uv!.Variant)
                                .ThenInclude(v => v!.VehicleModel)
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt!.PartCategory)
                .Where(x => x.Level == level && x.Status == ReminderStatus.Active);

            if (!includeAlreadyNotified)
                query = query.Where(x => !x.IsNotified);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdWithDetailsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt!.PartCategory)
                .Include(x => x.TrackingCycle)
                    .ThenInclude(tc => tc.PartTracking)
                        .ThenInclude(pt => pt!.UserVehicle)
                            .ThenInclude(uv => uv!.Variant)
                                .ThenInclude(v => v!.VehicleModel)
                .Where(x => x.TrackingCycle.PartTracking.UserVehicleId == userVehicleId && x.Status == ReminderStatus.Active)
                .ToListAsync(cancellationToken);
        }
    }
}
