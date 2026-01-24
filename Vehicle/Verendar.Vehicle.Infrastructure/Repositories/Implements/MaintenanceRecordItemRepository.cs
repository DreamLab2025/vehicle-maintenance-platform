using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordItemRepository : PostgresRepository<MaintenanceRecordItem>, IMaintenanceRecordItemRepository
    {
        public MaintenanceRecordItemRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceRecordItem>> GetByMaintenanceRecordIdAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Include(x => x.PartProduct)
                .Where(x => x.MaintenanceRecordId == maintenanceRecordId)
                .ToListAsync(cancellationToken);
        }
    }
}
