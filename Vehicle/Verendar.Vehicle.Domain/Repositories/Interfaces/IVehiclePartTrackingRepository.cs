using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehiclePartTrackingRepository : IGenericRepository<VehiclePartTracking>
    {
        Task<IEnumerable<VehiclePartTracking>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<VehiclePartTracking>> GetDeclaredByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<VehiclePartTracking?> GetByUserVehicleAndPartCategoryAsync(Guid userVehicleId, Guid partCategoryId, string? instanceIdentifier = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<VehiclePartTracking>> GetActiveTrackingsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
    }
}
