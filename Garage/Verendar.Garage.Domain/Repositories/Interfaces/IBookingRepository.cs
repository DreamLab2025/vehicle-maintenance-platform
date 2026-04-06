using Verendar.Common.Stats;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.Models;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<Booking?> GetByIdTrackedForMutationAsync(Guid id, CancellationToken ct = default);

    Task<BookingAssignmentSnapshot?> GetAssignmentSnapshotAsync(Guid id, CancellationToken ct = default);

    Task<bool> TryAssignMechanicPersistAsync(
        Guid bookingId,
        Guid mechanicMemberId,
        BookingStatus fromStatus,
        Guid actorId,
        CancellationToken ct = default);

    Task<bool> TryUpdateMechanicStatusPersistAsync(
        Guid bookingId,
        Guid mechanicMemberId,
        BookingStatus fromStatus,
        BookingStatus toStatus,
        Guid actorId,
        int? currentOdometer,
        CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        BookingStatus? status = null,
        CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        int pageNumber,
        int pageSize,
        BookingStatus? status = null,
        CancellationToken ct = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByMechanicMemberIdsAsync(
        IReadOnlyList<Guid> mechanicMemberIds,
        int pageNumber,
        int pageSize,
        BookingStatus? status = null,
        CancellationToken ct = default);

    Task<Booking?> GetByIdForAccessCheckAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, string>> GetItemsSummariesForBookingsAsync(
        IReadOnlyList<Guid> bookingIds,
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

    // ─── Admin / chart queries ────────────────────────────────────────────

    Task<BookingCountByStatus> GetAllBookingCountsByStatusAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<decimal> GetCompletedRevenueAsync(
        IReadOnlyList<Guid>? branchIds,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<List<ChartPoint>> GetBookingTrafficChartAsync(
        DateTime from,
        DateTime to,
        string groupBy,
        CancellationToken ct = default);

    Task<(List<string> Labels, List<decimal> CompletedData, List<decimal> CancelledData)> GetBookingOutcomesChartAsync(
        DateTime from,
        DateTime to,
        string groupBy,
        CancellationToken ct = default);

    Task<List<ChartPoint>> GetRevenueChartAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        string groupBy,
        CancellationToken ct = default);

    Task<List<TopServiceStats>> GetTopServicesAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime? from,
        DateTime? to,
        int limit,
        CancellationToken ct = default);

    Task<List<BranchBookingCount>> GetBranchBookingCountsAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);
}
