using Verender.Common.Databases.Interfaces;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleTypeBrandRepository : IGenericRepository<VehicleTypeBrand>
    {
        Task<List<VehicleBrand>> GetBrandsByTypeIdAsync(Guid typeId);
        Task<List<VehicleType>> GetTypesByBrandIdAsync(Guid brandId);
        Task<bool> ExistsAsync(Guid typeId, Guid brandId);
        Task<VehicleTypeBrand?> GetByTypeAndBrandAsync(Guid typeId, Guid brandId);
    }
}
