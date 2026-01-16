using Verender.Common.Databases.Interfaces;
using Verender.Vehicle.Domain.Entities;

namespace Verender.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehicleVariantRepository : IGenericRepository<VehicleVariant>
    {
        Task<IEnumerable<VehicleVariant>> GetImagesByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
        Task<VehicleVariant?> GetImageByVehicleModelIdAndColorAsync(Guid vehicleModelId, string color, CancellationToken cancellationToken = default);
    }
}
