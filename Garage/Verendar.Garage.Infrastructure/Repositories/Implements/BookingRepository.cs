namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class BookingRepository(GarageDbContext context)
    : PostgresRepository<Booking>(context), IBookingRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Set<Booking>()
            .AsSplitQuery()
            .AsNoTracking()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .Include(b => b.GarageProduct)
            .Include(b => b.Mechanic)
            .Include(b => b.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);
    }

    public async Task<Booking?> GetByIdTrackedWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Set<Booking>()
            .AsSplitQuery()
            .Include(b => b.GarageBranch)
                .ThenInclude(br => br.Garage)
            .Include(b => b.GarageProduct)
            .Include(b => b.Mechanic)
            .Include(b => b.StatusHistory)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.DeletedAt == null)
            .Include(b => b.GarageBranch)
            .Include(b => b.GarageProduct)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Set<Booking>()
            .AsNoTracking()
            .Where(b => b.GarageBranchId == branchId && b.DeletedAt == null)
            .Include(b => b.GarageBranch)
            .Include(b => b.GarageProduct)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByMechanicMemberIdsAsync(
        IReadOnlyList<Guid> mechanicMemberIds,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
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
            .Include(b => b.GarageProduct)
            .OrderByDescending(b => b.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
