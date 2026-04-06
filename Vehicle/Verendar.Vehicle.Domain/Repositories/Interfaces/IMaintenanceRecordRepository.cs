using Verendar.Common.Stats;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceRecordRepository : IGenericRepository<MaintenanceRecord>
    {
        Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdWithItemsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<MaintenanceRecord?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(int TotalCount, DateOnly? LastServiceDate)> GetActivitySummaryByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);

        Task<(int Total, decimal TotalCost)> GetStatsSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
        Task<List<ChartPoint>> GetActivityChartAsync(DateTime from, DateTime to, string groupBy, CancellationToken ct = default);
    }
}
