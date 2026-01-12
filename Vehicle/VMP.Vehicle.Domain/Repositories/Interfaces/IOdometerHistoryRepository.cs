using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOdometerHistoryRepository : IGenericRepository<OdometerHistory>
    {
        Task<int> GetCurrentStreakAsync(Guid userVehicleId);
    }
}
