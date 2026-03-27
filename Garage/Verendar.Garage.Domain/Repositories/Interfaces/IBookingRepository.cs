using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.Models;

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

    Task<RevenueStats> GetRevenueStatsAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        StatsPeriod period,
        CancellationToken ct = default);

    Task<BookingCountByStatus> GetBookingCountsByStatusAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    Task<List<TopItemStats>> GetTopItemsAsync(
        Guid branchId,
        DateTime from,
        DateTime to,
        int limit,
        CancellationToken ct = default);

    Task<List<MechanicStats>> GetMechanicStatsAsync(
        Guid branchId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    Task<List<BranchBookingSummary>> GetBranchSummariesAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}
