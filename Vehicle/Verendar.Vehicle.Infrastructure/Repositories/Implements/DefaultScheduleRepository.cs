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

        public async Task<DefaultMaintenanceSchedule?> GetByVehicleModelIdAndPartCategorySlugAsync(
            Guid vehicleModelId,
            string partCategorySlug,
            CancellationToken cancellationToken = default)
        {
            var slug = partCategorySlug.Trim();
            return await _dbSet
                .Include(x => x.PartCategory)
                .FirstOrDefaultAsync(
                    x => x.VehicleModelId == vehicleModelId
                        && x.PartCategory != null
                        && x.PartCategory.Slug.ToLower() == slug.ToLower(),
                    cancellationToken);
        }
    }
}
