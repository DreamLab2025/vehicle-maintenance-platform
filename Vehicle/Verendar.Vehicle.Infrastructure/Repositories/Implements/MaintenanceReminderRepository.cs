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
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetPendingRemindersAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(x => x.PartCategory)
                .Where(x =>
                    x.PartTracking.UserVehicleId == userVehicleId &&
                    x.IsCurrent &&
                    !x.IsNotified &&
                    !x.IsDismissed)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByLevelAsync(ReminderLevel level, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(x => x.PartCategory)
                .Where(x => x.Level == level && x.IsCurrent && !x.IsNotified)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByLevelWithDetailsAsync(
            ReminderLevel level,
            bool includeAlreadyNotified,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(pt => pt!.UserVehicle)
                        .ThenInclude(uv => uv!.Variant)
                            .ThenInclude(v => v!.VehicleModel)
                .Include(x => x.PartTracking)
                    .ThenInclude(pt => pt!.PartCategory)
                .Where(x => x.Level == level && x.IsCurrent);

            if (!includeAlreadyNotified)
                query = query.Where(x => !x.IsNotified);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByUserVehicleIdWithDetailsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartTracking)
                    .ThenInclude(pt => pt!.PartCategory)
                .Include(x => x.PartTracking)
                    .ThenInclude(pt => pt!.UserVehicle)
                        .ThenInclude(uv => uv!.Variant)
                            .ThenInclude(v => v!.VehicleModel)
                .Where(x => x.PartTracking!.UserVehicleId == userVehicleId && x.IsCurrent)
                .ToListAsync(cancellationToken);
        }
    }
}
