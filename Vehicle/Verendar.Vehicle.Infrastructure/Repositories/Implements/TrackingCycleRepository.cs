using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class TrackingCycleRepository(VehicleDbContext context) : PostgresRepository<TrackingCycle>(context), ITrackingCycleRepository
    {
        public async Task<IEnumerable<TrackingCycle>> GetActiveCyclesByVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.PartTracking)
                    .ThenInclude(pt => pt.PartCategory)
                .Include(c => c.Reminders)
                .Where(c => c.PartTracking.UserVehicleId == userVehicleId && c.Status == CycleStatus.Active)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TrackingCycle>> GetByPartTrackingIdAsync(Guid partTrackingId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Reminders)
                .Where(c => c.PartTrackingId == partTrackingId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
