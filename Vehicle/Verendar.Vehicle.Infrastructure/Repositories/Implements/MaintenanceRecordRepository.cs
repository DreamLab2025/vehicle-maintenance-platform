using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordRepository : PostgresRepository<MaintenanceRecord>, IMaintenanceRecordRepository
    {
        public MaintenanceRecordRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.ServiceDate)
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
    }
}
