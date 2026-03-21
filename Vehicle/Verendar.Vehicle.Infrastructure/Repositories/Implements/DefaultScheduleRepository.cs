using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class DefaultScheduleRepository(VehicleDbContext context) : PostgresRepository<DefaultMaintenanceSchedule>(context), IDefaultScheduleRepository
    {
        public async Task<IEnumerable<DefaultMaintenanceSchedule>> GetByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Where(x => x.VehicleModelId == vehicleModelId)
                .ToListAsync(cancellationToken);
        }

    }
}
