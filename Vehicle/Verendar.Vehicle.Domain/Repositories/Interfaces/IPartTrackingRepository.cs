using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartTrackingRepository : IGenericRepository<PartTracking>
    {
        Task<IEnumerable<PartTracking>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTracking>> GetDeclaredByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<PartTracking?> GetByUserVehicleAndPartCategoryAsync(Guid userVehicleId, Guid partCategoryId, string? instanceIdentifier = null, CancellationToken cancellationToken = default);
        Task<PartTracking?> GetFirstByUserVehicleAndPartCategoryAsync(Guid userVehicleId, Guid partCategoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTracking>> GetActiveTrackingsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PartTracking>> GetByIdsWithCyclesAndRemindersAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    }
}
