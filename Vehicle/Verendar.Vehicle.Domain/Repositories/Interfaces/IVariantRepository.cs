using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVariantRepository : IGenericRepository<Variant>
    {
        Task<Variant?> GetByIdWithVehicleModelAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Variant>> GetImagesByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
        Task<Variant?> GetImageByVehicleModelIdAndColorAsync(Guid vehicleModelId, string color, CancellationToken cancellationToken = default);
    }
}
