using Verendar.Garage.Domain.Models;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageBranchRepository(GarageDbContext context)
    : PostgresRepository<GarageBranch>(context), IGarageBranchRepository
{
    public async Task<BranchCounts> GetBranchCountsAsync(CancellationToken ct = default)
    {
        var counts = await context.Set<GarageBranch>()
            .AsNoTracking()
            .Where(b => b.DeletedAt == null)
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var active = counts.FirstOrDefault(c => c.Status == BranchStatus.Active)?.Count ?? 0;
        var inactive = counts.FirstOrDefault(c => c.Status == BranchStatus.Inactive)?.Count ?? 0;
        return new BranchCounts(Total: active + inactive, Active: active, Inactive: inactive);
    }

    public async Task<Guid?> GetGarageOwnerIdByBranchIdAsync(Guid branchId, CancellationToken ct = default) =>
        await context.Set<GarageBranch>()
            .AsNoTracking()
            .Where(b => b.Id == branchId && b.DeletedAt == null)
            .Select(b => (Guid?)b.Garage.OwnerId)
            .FirstOrDefaultAsync(ct);

    public async Task<(List<GarageBranch> Items, int TotalCount)> GetBranchesForMapAsync(
        int pageNumber,
        int pageSize,
        double? minLat = null,
        double? maxLat = null,
        double? minLng = null,
        double? maxLng = null,
        CancellationToken ct = default)
    {
        var query = context.Set<GarageBranch>()
            .Include(b => b.Garage)
            .Where(b => b.DeletedAt == null
                     && b.Garage.DeletedAt == null
                     && b.Status == BranchStatus.Active
                     && b.Latitude != null
                     && b.Longitude != null);

        if (minLat.HasValue) query = query.Where(b => b.Latitude >= minLat.Value);
        if (maxLat.HasValue) query = query.Where(b => b.Latitude <= maxLat.Value);
        if (minLng.HasValue) query = query.Where(b => b.Longitude >= minLng.Value);
        if (maxLng.HasValue) query = query.Where(b => b.Longitude <= maxLng.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
