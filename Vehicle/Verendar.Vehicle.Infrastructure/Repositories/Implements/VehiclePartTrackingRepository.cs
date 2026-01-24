using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehiclePartTrackingRepository : PostgresRepository<VehiclePartTracking>, IVehiclePartTrackingRepository
    {
        public VehiclePartTrackingRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VehiclePartTracking>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .Where(x => x.UserVehicleId == userVehicleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<VehiclePartTracking?> GetByUserVehicleAndPartCategoryAsync(Guid userVehicleId, Guid partCategoryId, string? instanceIdentifier = null, CancellationToken cancellationToken = default)
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

        public async Task<IEnumerable<VehiclePartTracking>> GetActiveTrackingsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.CurrentPartProduct)
                .Where(x => x.UserVehicleId == userVehicleId && !x.IsIgnored)
                .ToListAsync(cancellationToken);
        }
    }
}
