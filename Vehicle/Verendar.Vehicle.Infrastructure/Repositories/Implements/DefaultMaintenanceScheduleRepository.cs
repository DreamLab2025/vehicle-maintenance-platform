using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class DefaultMaintenanceScheduleRepository : PostgresRepository<DefaultMaintenanceSchedule>, IDefaultMaintenanceScheduleRepository
    {
        public DefaultMaintenanceScheduleRepository(VehicleDbContext context) : base(context)
        {
        }

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
