using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordRepository(VehicleDbContext context) : PostgresRepository<MaintenanceRecord>(context), IMaintenanceRecordRepository
    {
        public async Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.ServiceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdWithItemsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.ServiceDate)
                .Include(x => x.Items)
                    .ThenInclude(i => i.PartCategory)
                .Include(x => x.Items)
                    .ThenInclude(i => i.PartProduct)
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceRecord?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Items)
                    .ThenInclude(x => x.PartCategory)
                .Include(x => x.Items)
                    .ThenInclude(x => x.PartProduct)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(int TotalCount, DateOnly? LastServiceDate)> GetActivitySummaryByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(r => r.UserVehicleId == userVehicleId);
            var count = await query.CountAsync(cancellationToken);
            if (count == 0)
                return (0, null);
            var last = await query.MaxAsync(r => r.ServiceDate, cancellationToken);
            return (count, last);
        }
    }
}
