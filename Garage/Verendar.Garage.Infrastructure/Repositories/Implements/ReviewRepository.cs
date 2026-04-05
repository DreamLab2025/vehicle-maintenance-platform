namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class ReviewRepository(GarageDbContext context)
    : PostgresRepository<GarageReview>(context), IReviewRepository
{
    public async Task<(double AverageRating, int ReviewCount)> GetRatingSummaryAsync(
        Guid branchId, CancellationToken ct = default)
    {
        var stats = await context.GarageReviews
            .Where(r => r.GarageBranchId == branchId && r.DeletedAt == null)
            .GroupBy(_ => 1)
            .Select(g => new { Avg = (double?)g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return stats is null ? (0, 0) : (stats.Avg ?? 0, stats.Count);
    }

    public async Task<Dictionary<Guid, (double AverageRating, int ReviewCount)>> GetBulkRatingSummaryAsync(
        IEnumerable<Guid> branchIds, CancellationToken ct = default)
    {
        var ids = branchIds.ToList();
        if (ids.Count == 0) return new();

        return await context.GarageReviews
            .Where(r => ids.Contains(r.GarageBranchId) && r.DeletedAt == null)
            .GroupBy(r => r.GarageBranchId)
            .Select(g => new { BranchId = g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => (x.Avg, x.Count), ct);
    }

    public async Task<(double AvgRating, int TotalCount)> GetGlobalRatingSummaryAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = context.GarageReviews.Where(r => r.DeletedAt == null);
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value);

        var stats = await query
            .GroupBy(_ => 1)
            .Select(g => new { Avg = (double?)g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return stats is null ? (0, 0) : (stats.Avg ?? 0, stats.Count);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(
        IEnumerable<Guid> branchIds, CancellationToken ct = default)
    {
        var ids = branchIds.ToList();
        if (ids.Count == 0) return new();

        return await context.GarageReviews
            .Where(r => ids.Contains(r.GarageBranchId) && r.DeletedAt == null)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, ct);
    }
}
