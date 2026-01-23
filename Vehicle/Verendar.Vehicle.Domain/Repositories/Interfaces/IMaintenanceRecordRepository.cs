using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceRecordRepository : IGenericRepository<MaintenanceRecord>
    {
        Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<MaintenanceRecord?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
