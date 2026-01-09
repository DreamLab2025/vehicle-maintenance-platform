using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleVariantRepository : IGenericRepository<VehicleVariant>
    {
        Task<IEnumerable<VehicleVariant>> GetImagesByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
        Task<VehicleVariant?> GetImageByVehicleModelIdAndColorAsync(Guid vehicleModelId, string color, CancellationToken cancellationToken = default);
    }
}
