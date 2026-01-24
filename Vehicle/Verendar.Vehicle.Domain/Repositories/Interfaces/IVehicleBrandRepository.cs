using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleBrandRepository : IGenericRepository<VehicleBrand>
    {
        Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id);
    }
}
