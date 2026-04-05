using System.Globalization;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.Models;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class BookingRepository(GarageDbContext context)
    : PostgresRepository<Booking>(context), IBookingRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<Booking>()
            .AsSplitQuery()
            .AsNoTracking()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .Include(b => b.Mechanic)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.InstallationService)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Service)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Bundle)
                    .ThenInclude(bu => bu!.Items.Where(bi => bi.DeletedAt == null))
                        .ThenInclude(bi => bi.Product)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Bundle)
                    .ThenInclude(bu => bu!.Items.Where(bi => bi.DeletedAt == null))
                        .ThenInclude(bi => bi.Service)
            .Include(b => b.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<Booking?> GetByIdTrackedWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<Booking>()
            .AsSplitQuery()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .Include(b => b.Mechanic)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Product)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Service)
            .Include(b => b.LineItems.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Bundle)
            .Include(b => b.StatusHistory)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<Booking?> GetByIdTrackedForMutationAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<Booking>()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<BookingAssignmentSnapshot?> GetAssignmentSnapshotAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.Id == id && b.DeletedAt == null)
            .Select(b => new BookingAssignmentSnapshot(b.Id, b.Status, b.GarageBranchId, b.UserId, b.ScheduledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<bool> TryAssignMechanicPersistAsync(
        Guid bookingId,
        Guid mechanicMemberId,
        BookingStatus fromStatus,
        Guid actorId,
        CancellationToken ct = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async cancellationToken =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var now = DateTime.UtcNow;
                    var rows = await _db.Set<Booking>()
                        .Where(b => b.Id == bookingId && b.DeletedAt == null && b.Status == fromStatus)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(b => b.MechanicId, mechanicMemberId)
                            .SetProperty(b => b.Status, BookingStatus.Confirmed)
                            .SetProperty(b => b.UpdatedAt, now)
                            .SetProperty(b => b.UpdatedBy, actorId),
                            cancellationToken);

                    if (rows == 0)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        return false;
                    }

                    await _db.Set<BookingStatusHistory>().AddAsync(
                        new BookingStatusHistory
                        {
                            BookingId = bookingId,
                            FromStatus = fromStatus,
                            ToStatus = BookingStatus.Confirmed,
                            ChangedByUserId = actorId,
                            ChangedAt = now
                        },
                        cancellationToken);

                    await _db.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    return true;
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            },
            ct);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.DeletedAt == null)
            .Include(b => b.GarageBranch)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.GarageBranchId == branchId && b.DeletedAt == null)
            .Include(b => b.GarageBranch)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByMechanicMemberIdsAsync(
        IReadOnlyList<Guid> mechanicMemberIds, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        if (mechanicMemberIds.Count == 0)
            return ([], 0);

        var query = _db.Set<Booking>()
            .AsNoTracking()
            .Where(b =>
                b.MechanicId.HasValue
                && mechanicMemberIds.Contains(b.MechanicId.Value)
                && b.DeletedAt == null)
            .Include(b => b.GarageBranch)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Booking?> GetByIdForAccessCheckAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<Booking>()
            .AsNoTracking()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetItemsSummariesForBookingsAsync(
        IReadOnlyList<Guid> bookingIds,
        CancellationToken ct = default)
    {
        if (bookingIds.Count == 0)
            return new Dictionary<Guid, string>();

        var distinctIds = bookingIds.Distinct().ToList();

        var lineItems = await _db.Set<BookingLineItem>()
            .AsNoTracking()
            .Where(li => distinctIds.Contains(li.BookingId))
            .Include(li => li.Product)
            .Include(li => li.Service)
            .Include(li => li.Bundle)
            .ToListAsync(ct);

        var result = distinctIds.ToDictionary(i => i, _ => string.Empty);

        foreach (var group in lineItems.GroupBy(li => li.BookingId))
            result[group.Key] = BookingMappings.BuildItemsSummaryFromLineItems(group.ToList());

        return result;
    }

    public async Task<RevenueStats> GetRevenueStatsAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        StatsPeriod period,
        CancellationToken ct = default)
    {
        var bookings = await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => branchIds.Contains(b.GarageBranchId)
                     && b.ScheduledAt >= from
                     && b.ScheduledAt <= to
                     && b.DeletedAt == null)
            .Select(b => new { b.ScheduledAt, b.Status, Amount = b.BookedTotalPrice.Amount })
            .ToListAsync(ct);

        var byStatus = new RevenueByStatus(
            Completed: bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.Amount),
            InProgress: bookings.Where(b => b.Status == BookingStatus.InProgress).Sum(b => b.Amount),
            Confirmed: bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.Amount),
            AwaitingConfirmation: bookings.Where(b => b.Status == BookingStatus.AwaitingConfirmation).Sum(b => b.Amount),
            Pending: bookings.Where(b => b.Status == BookingStatus.Pending).Sum(b => b.Amount),
            Cancelled: bookings.Where(b => b.Status == BookingStatus.Cancelled).Sum(b => b.Amount)
        );

        var periodLength = to - from;
        var prevTo = from.AddSeconds(-1);
        var prevFrom = prevTo - periodLength;

        var previousCompleted = await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => branchIds.Contains(b.GarageBranchId)
                     && b.ScheduledAt >= prevFrom
                     && b.ScheduledAt <= prevTo
                     && b.Status == BookingStatus.Completed
                     && b.DeletedAt == null)
            .SumAsync(b => (decimal?)b.BookedTotalPrice.Amount, ct) ?? 0;

        var trend = BuildTrend(bookings.Select(b => (b.ScheduledAt, b.Status, b.Amount)).ToList(), period);

        return new RevenueStats(byStatus, previousCompleted, trend);
    }

    public async Task<BookingCountByStatus> GetBookingCountsByStatusAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var counts = await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => branchIds.Contains(b.GarageBranchId)
                     && b.ScheduledAt >= from
                     && b.ScheduledAt <= to
                     && b.DeletedAt == null)
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return new BookingCountByStatus(
            Completed: counts.FirstOrDefault(c => c.Status == BookingStatus.Completed)?.Count ?? 0,
            InProgress: counts.FirstOrDefault(c => c.Status == BookingStatus.InProgress)?.Count ?? 0,
            Confirmed: counts.FirstOrDefault(c => c.Status == BookingStatus.Confirmed)?.Count ?? 0,
            AwaitingConfirmation: counts.FirstOrDefault(c => c.Status == BookingStatus.AwaitingConfirmation)?.Count ?? 0,
            Pending: counts.FirstOrDefault(c => c.Status == BookingStatus.Pending)?.Count ?? 0,
            Cancelled: counts.FirstOrDefault(c => c.Status == BookingStatus.Cancelled)?.Count ?? 0
        );
    }

    public async Task<List<TopItemStats>> GetTopItemsAsync(
        Guid branchId,
        DateTime from,
        DateTime to,
        int limit,
        CancellationToken ct = default)
    {
        var lineItems = await _db.Set<BookingLineItem>()
            .AsNoTracking()
            .Where(li => li.Booking.GarageBranchId == branchId
                      && li.Booking.ScheduledAt >= from
                      && li.Booking.ScheduledAt <= to
                      && li.Booking.DeletedAt == null
                      && li.DeletedAt == null)
            .Select(li => new
            {
                li.BookingId,
                li.ProductId,
                li.ServiceId,
                li.BundleId,
                ProductName = li.Product != null ? li.Product.Name : null,
                ServiceName = li.Service != null ? li.Service.Name : null,
                BundleName = li.Bundle != null ? li.Bundle.Name : null,
                BookingStatus = li.Booking.Status,
                Amount = li.BookedItemPrice.Amount
            })
            .ToListAsync(ct);

        return lineItems
            .GroupBy(li => new
            {
                li.ProductId,
                li.ServiceId,
                li.BundleId,
                Name = li.ProductName ?? li.ServiceName ?? li.BundleName ?? "Unknown"
            })
            .Select(g =>
            {
                var itemType = g.Key.ProductId.HasValue ? "Product"
                    : g.Key.ServiceId.HasValue ? "Service"
                    : "Bundle";
                return new TopItemStats(
                    ProductId: g.Key.ProductId,
                    ServiceId: g.Key.ServiceId,
                    BundleId: g.Key.BundleId,
                    ItemName: g.Key.Name,
                    ItemType: itemType,
                    BookingCount: g.Select(li => li.BookingId).Distinct().Count(),
                    CompletedRevenue: g.Where(li => li.BookingStatus == BookingStatus.Completed).Sum(li => li.Amount),
                    CancelledRevenue: g.Where(li => li.BookingStatus == BookingStatus.Cancelled).Sum(li => li.Amount)
                );
            })
            .OrderByDescending(t => t.CompletedRevenue)
            .Take(limit)
            .ToList();
    }

    public async Task<List<MechanicStats>> GetMechanicStatsAsync(
        Guid branchId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var results = await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.GarageBranchId == branchId
                     && b.ScheduledAt >= from
                     && b.ScheduledAt <= to
                     && b.Status == BookingStatus.Completed
                     && b.MechanicId.HasValue
                     && b.DeletedAt == null)
            .Join(
                _db.Set<GarageMember>().Where(m => m.DeletedAt == null),
                b => b.MechanicId,
                m => m.Id,
                (b, m) => new { MemberId = m.Id, m.DisplayName, Amount = b.BookedTotalPrice.Amount })
            .GroupBy(x => new { x.MemberId, x.DisplayName })
            .Select(g => new
            {
                g.Key.MemberId,
                g.Key.DisplayName,
                CompletedBookings = g.Count(),
                CompletedRevenue = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.CompletedRevenue)
            .ToListAsync(ct);

        return results
            .Select(x => new MechanicStats(x.MemberId, x.DisplayName, x.CompletedBookings, x.CompletedRevenue))
            .ToList();
    }

    public async Task<List<BranchBookingSummary>> GetBranchSummariesAsync(
        IReadOnlyList<Guid> branchIds,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var data = await _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => branchIds.Contains(b.GarageBranchId)
                     && b.ScheduledAt >= from
                     && b.ScheduledAt <= to
                     && b.DeletedAt == null)
            .GroupBy(b => new { b.GarageBranchId, b.Status })
            .Select(g => new
            {
                g.Key.GarageBranchId,
                g.Key.Status,
                Count = g.Count(),
                Revenue = g.Sum(b => b.BookedTotalPrice.Amount)
            })
            .ToListAsync(ct);

        return branchIds.Select(branchId =>
        {
            var bd = data.Where(d => d.GarageBranchId == branchId).ToList();

            var revenue = new RevenueByStatus(
                Completed: bd.FirstOrDefault(d => d.Status == BookingStatus.Completed)?.Revenue ?? 0,
                InProgress: bd.FirstOrDefault(d => d.Status == BookingStatus.InProgress)?.Revenue ?? 0,
                Confirmed: bd.FirstOrDefault(d => d.Status == BookingStatus.Confirmed)?.Revenue ?? 0,
                AwaitingConfirmation: bd.FirstOrDefault(d => d.Status == BookingStatus.AwaitingConfirmation)?.Revenue ?? 0,
                Pending: bd.FirstOrDefault(d => d.Status == BookingStatus.Pending)?.Revenue ?? 0,
                Cancelled: bd.FirstOrDefault(d => d.Status == BookingStatus.Cancelled)?.Revenue ?? 0
            );

            var counts = new BookingCountByStatus(
                Completed: bd.FirstOrDefault(d => d.Status == BookingStatus.Completed)?.Count ?? 0,
                InProgress: bd.FirstOrDefault(d => d.Status == BookingStatus.InProgress)?.Count ?? 0,
                Confirmed: bd.FirstOrDefault(d => d.Status == BookingStatus.Confirmed)?.Count ?? 0,
                AwaitingConfirmation: bd.FirstOrDefault(d => d.Status == BookingStatus.AwaitingConfirmation)?.Count ?? 0,
                Pending: bd.FirstOrDefault(d => d.Status == BookingStatus.Pending)?.Count ?? 0,
                Cancelled: bd.FirstOrDefault(d => d.Status == BookingStatus.Cancelled)?.Count ?? 0
            );

            return new BranchBookingSummary(branchId, revenue, counts);
        }).ToList();
    }

    private static List<TrendPoint> BuildTrend(
        List<(DateTime ScheduledAt, BookingStatus Status, decimal Amount)> bookings,
        StatsPeriod period)
    {
        Func<DateTime, string> labelSelector = period switch
        {
            StatsPeriod.Weekly => d =>
            {
                var week = ISOWeek.GetWeekOfYear(d);
                var year = ISOWeek.GetYear(d);
                return $"{year:0000}-W{week:00}";
            },
            StatsPeriod.Quarterly => d => $"{d.Year:0000}-Q{(d.Month - 1) / 3 + 1}",
            _ => d => $"{d.Year:0000}-{d.Month:00}"
        };

        return bookings
            .GroupBy(b => labelSelector(b.ScheduledAt))
            .Select(g => new TrendPoint(
                Label: g.Key,
                Completed: g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.Amount),
                Cancelled: g.Where(b => b.Status == BookingStatus.Cancelled).Sum(b => b.Amount),
                Other: g.Where(b => b.Status is not BookingStatus.Completed and not BookingStatus.Cancelled).Sum(b => b.Amount)
            ))
            .OrderBy(t => t.Label)
            .ToList();
    }
}
