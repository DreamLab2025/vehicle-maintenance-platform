using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUserVehicleRepository : IGenericRepository<UserVehicle>
    {
        IQueryable<UserVehicle> GetQueryWithFullDetails();
        Task<UserVehicle?> GetByIdWithFullDetailsAsync(Guid id);
        Task<UserVehicle?> GetByIdAndUserIdWithFullDetailsAsync(Guid id, Guid userId);
    }
}
