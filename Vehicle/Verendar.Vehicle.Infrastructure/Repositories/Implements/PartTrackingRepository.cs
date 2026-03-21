using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class PartTrackingRepository(VehicleDbContext context) : PostgresRepository<PartTracking>(context), IPartTrackingRepository
    {
        public async Task<IEnumerable<PartTracking>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .Include(x => x.Cycles)
                .Where(x => x.UserVehicleId == userVehicleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PartTracking>> GetDeclaredByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .Include(x => x.Cycles)
                    .ThenInclude(c => c.Reminders)
                .Where(x => x.UserVehicleId == userVehicleId && x.IsDeclared)
                .ToListAsync(cancellationToken);
        }

        public async Task<PartTracking?> GetByUserVehicleAndPartCategoryAsync(Guid userVehicleId, Guid partCategoryId, string? instanceIdentifier = null, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .FirstOrDefaultAsync(x =>
                    x.UserVehicleId == userVehicleId &&
                    x.PartCategoryId == partCategoryId &&
                    x.InstanceIdentifier == instanceIdentifier,
                    cancellationToken);
        }

        public async Task<IEnumerable<PartTracking>> GetActiveTrackingsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .Where(x => x.UserVehicleId == userVehicleId && !x.IsDeclared)
                .ToListAsync(cancellationToken);
        }
    }
}
