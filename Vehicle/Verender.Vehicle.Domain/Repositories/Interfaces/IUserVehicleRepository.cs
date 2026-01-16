using Verender.Common.Databases.Interfaces;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUserVehicleRepository : IGenericRepository<UserVehicle>
    {
        IQueryable<UserVehicle> GetQueryWithFullDetails();
        Task<UserVehicle?> GetByIdWithFullDetailsAsync(Guid id);
        Task<UserVehicle?> GetByIdAndUserIdWithFullDetailsAsync(Guid id, Guid userId);
        Task<(bool IsAllowed, string Message)> CheckCanCreateVehicleAsync(Guid userId, bool isPremiumUser = false);
    }
}
