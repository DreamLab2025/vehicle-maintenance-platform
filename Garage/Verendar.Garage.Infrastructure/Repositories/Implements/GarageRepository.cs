using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.Models;
using GarageEntity = global::Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageRepository(GarageDbContext context)
    : PostgresRepository<GarageEntity>(context), IGarageRepository
{
    public async Task<GarageEntity?> GetWithBranchesAsync(
        Expression<Func<GarageEntity, bool>> filter,
        CancellationToken ct = default)
    {
        return await context.Set<GarageEntity>()
            .Include(g => g.Branches)
            .Where(g => g.DeletedAt == null)
            .FirstOrDefaultAsync(filter, ct);
    }

    public async Task<GarageStatusCounts> GetStatusCountsAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        var query = context.Set<GarageEntity>()
            .AsNoTracking()
            .Where(g => g.DeletedAt == null);

        if (from.HasValue) query = query.Where(g => g.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(g => g.CreatedAt <= to.Value);

        var counts = await query
            .GroupBy(g => g.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = counts.Sum(c => c.Count);
        return new GarageStatusCounts(
            Total: total,
            Pending: counts.FirstOrDefault(c => c.Status == GarageStatus.Pending)?.Count ?? 0,
            Active: counts.FirstOrDefault(c => c.Status == GarageStatus.Active)?.Count ?? 0,
            Suspended: counts.FirstOrDefault(c => c.Status == GarageStatus.Suspended)?.Count ?? 0,
            Rejected: counts.FirstOrDefault(c => c.Status == GarageStatus.Rejected)?.Count ?? 0
        );
    }
}
