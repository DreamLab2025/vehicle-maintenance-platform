using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class DefaultMaintenanceScheduleRepository(VehicleDbContext context) : PostgresRepository<DefaultMaintenanceSchedule>(context), IDefaultMaintenanceScheduleRepository
    {
        public async Task<IEnumerable<DefaultMaintenanceSchedule>> GetByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Where(x => x.VehicleModelId == vehicleModelId)
                .ToListAsync(cancellationToken);
        }

        public async Task<DefaultMaintenanceSchedule?> GetByVehicleModelAndPartCategoryAsync(Guid vehicleModelId, Guid partCategoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .FirstOrDefaultAsync(x => x.VehicleModelId == vehicleModelId && x.PartCategoryId == partCategoryId, cancellationToken);
        }
    }
}
