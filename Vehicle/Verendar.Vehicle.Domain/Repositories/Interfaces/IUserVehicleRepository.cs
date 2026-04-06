using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IUserVehicleRepository : IGenericRepository<UserVehicle>
    {
        IQueryable<UserVehicle> GetQueryWithFullDetails();
        IQueryable<UserVehicle> GetQueryWithoutPartTrackings();
        Task<UserVehicle?> GetByIdWithFullDetailsAsync(Guid id);
        Task<UserVehicle?> GetByIdAndUserIdWithFullDetailsAsync(Guid id, Guid userId);
        Task<UserVehicle?> GetByIdAndUserIdWithoutPartTrackingsAsync(Guid id, Guid userId);
        Task<(bool IsAllowed, string Message)> CheckCanCreateVehicleAsync(Guid userId, bool isPremiumUser = false);
        Task<IReadOnlyList<Guid>> GetDistinctUserIdsWithStaleOdometerAsync(int olderThanDays, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserVehicle>> GetStaleOdometerVehiclesByUserAsync(Guid userId, int olderThanDays, CancellationToken cancellationToken = default);
    }
}
