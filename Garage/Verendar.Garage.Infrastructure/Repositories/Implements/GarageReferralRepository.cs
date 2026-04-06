namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageReferralRepository(GarageDbContext context)
    : PostgresRepository<GarageReferral>(context), IGarageReferralRepository
{
    public async Task<bool> ExistsAsync(Guid garageId, Guid userId, CancellationToken ct = default)
    {
        return await context.GarageReferrals
            .AnyAsync(r => r.GarageId == garageId && r.ReferredUserId == userId && r.DeletedAt == null, ct);
    }

    public async Task<(List<GarageReferral> Items, int TotalCount)> GetPagedByGarageAsync(
        Guid garageId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = context.GarageReferrals
            .Where(r => r.GarageId == garageId && r.DeletedAt == null);

        if (from.HasValue) query = query.Where(r => r.ReferredAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.ReferredAt <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReferredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> CountByGarageAsync(Guid garageId, CancellationToken ct = default)
    {
        return await context.GarageReferrals
            .CountAsync(r => r.GarageId == garageId && r.DeletedAt == null, ct);
    }
}
