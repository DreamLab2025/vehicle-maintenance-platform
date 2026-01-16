using Verender.Common.Databases.Interfaces;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOdometerHistoryRepository : IGenericRepository<OdometerHistory>
    {
        Task<int> GetCurrentStreakAsync(Guid userVehicleId);
    }
}
