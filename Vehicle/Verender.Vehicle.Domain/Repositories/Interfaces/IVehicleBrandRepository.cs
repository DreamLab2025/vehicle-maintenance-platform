using Verender.Common.Databases.Interfaces;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleBrandRepository : IGenericRepository<VehicleBrand>
    {
        Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id);
    }
}
