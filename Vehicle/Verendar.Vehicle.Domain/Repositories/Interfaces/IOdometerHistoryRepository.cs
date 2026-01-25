using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOdometerHistoryRepository : IGenericRepository<OdometerHistory>
    {
        Task<int> GetCurrentStreakAsync(Guid userVehicleId);
    }
}
