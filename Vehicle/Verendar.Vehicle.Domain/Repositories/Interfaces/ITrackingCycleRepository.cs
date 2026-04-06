using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface ITrackingCycleRepository : IGenericRepository<TrackingCycle>
    {
        Task<IEnumerable<TrackingCycle>> GetActiveCyclesByVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TrackingCycle>> GetByPartTrackingIdAsync(Guid partTrackingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TrackingCycle>> GetActiveWithRemindersByPartTrackingIdAsync(Guid partTrackingId, CancellationToken cancellationToken = default);
    }
}
