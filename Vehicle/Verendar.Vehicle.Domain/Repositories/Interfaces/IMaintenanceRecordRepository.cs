using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceRecordRepository : IGenericRepository<MaintenanceRecord>
    {
        Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdWithItemsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<MaintenanceRecord?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Count of records for vehicle and latest <see cref="MaintenanceRecord.ServiceDate"/> when count &gt; 0.</summary>
        Task<(int TotalCount, DateOnly? LastServiceDate)> GetActivitySummaryByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
    }
}
