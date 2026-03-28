using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordItemRepository(VehicleDbContext context) : PostgresRepository<MaintenanceRecordItem>(context), IMaintenanceRecordItemRepository
    {
        public async Task<IEnumerable<MaintenanceRecordItem>> GetByMaintenanceRecordIdAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Where(x => x.MaintenanceRecordId == maintenanceRecordId)
                .ToListAsync(cancellationToken);
        }
    }
}
