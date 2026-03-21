using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOdometerHistoryRepository : IGenericRepository<OdometerHistory>
    {
        Task<int> GetCurrentStreakAsync(Guid userVehicleId);
        Task<(IEnumerable<OdometerHistory> Items, int TotalCount)> GetPagedByUserVehicleAsync(
            Guid userVehicleId,
            int pageNumber,
            int pageSize,
            DateOnly? fromDate,
            DateOnly? toDate,
            bool isDescending = true,
            CancellationToken cancellationToken = default);
    }
}
