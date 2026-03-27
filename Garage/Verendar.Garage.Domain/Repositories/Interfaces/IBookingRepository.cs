using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<Booking?> GetByIdTrackedWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByMechanicMemberIdsAsync(
        IReadOnlyList<Guid> mechanicMemberIds,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}
