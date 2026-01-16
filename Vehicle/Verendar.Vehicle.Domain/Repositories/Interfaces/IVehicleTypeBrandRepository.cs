using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleTypeBrandRepository : IGenericRepository<VehicleTypeBrand>
    {
        Task<List<VehicleBrand>> GetBrandsByTypeIdAsync(Guid typeId);
        Task<List<VehicleType>> GetTypesByBrandIdAsync(Guid brandId);
        Task<bool> ExistsAsync(Guid typeId, Guid brandId);
        Task<VehicleTypeBrand?> GetByTypeAndBrandAsync(Guid typeId, Guid brandId);
    }
}
