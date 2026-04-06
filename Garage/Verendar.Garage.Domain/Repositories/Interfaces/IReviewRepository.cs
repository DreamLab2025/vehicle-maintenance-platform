namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IReviewRepository : IGenericRepository<GarageReview>
{
    Task<(double AverageRating, int ReviewCount)> GetRatingSummaryAsync(
        Guid branchId, CancellationToken ct = default);

    Task<Dictionary<Guid, (double AverageRating, int ReviewCount)>> GetBulkRatingSummaryAsync(
        IEnumerable<Guid> branchIds, CancellationToken ct = default);

    Task<(double AvgRating, int TotalCount)> GetGlobalRatingSummaryAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default);

    Task<Dictionary<int, int>> GetRatingDistributionAsync(
        IEnumerable<Guid> branchIds, CancellationToken ct = default);
}
