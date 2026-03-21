using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceRecordItemRepository : IGenericRepository<MaintenanceRecordItem>
    {
        Task<IEnumerable<MaintenanceRecordItem>> GetByMaintenanceRecordIdAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default);
    }
}
