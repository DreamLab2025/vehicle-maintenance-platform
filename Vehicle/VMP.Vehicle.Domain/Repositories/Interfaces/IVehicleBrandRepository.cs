using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleBrandRepository : IGenericRepository<VehicleBrand>
    {
        Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id);
    }
}
