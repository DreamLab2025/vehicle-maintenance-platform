using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceReminderRepository(VehicleDbContext context) : PostgresRepository<MaintenanceReminder>(context), IMaintenanceReminderRepository
    {
        public async Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(x => x.PartCategory)
                .Where(x => x.PartTracking.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.Level)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetPendingRemindersAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(x => x.PartCategory)
                .Where(x =>
                    x.PartTracking.UserVehicleId == userVehicleId &&
                    !x.IsNotified &&
                    !x.IsDismissed)
                .OrderByDescending(x => x.Level)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByLevelAsync(ReminderLevel level, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(x => x.PartCategory)
                .Where(x => x.Level == level && !x.IsNotified)
                .ToListAsync(cancellationToken);
        }
    }
}
