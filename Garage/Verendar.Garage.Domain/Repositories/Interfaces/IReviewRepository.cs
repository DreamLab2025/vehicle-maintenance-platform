using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IReviewRepository : IGenericRepository<GarageReview>
{
    Task<(double AverageRating, int ReviewCount)> GetRatingSummaryAsync(
        Guid branchId, CancellationToken ct = default);

    Task<Dictionary<Guid, (double AverageRating, int ReviewCount)>> GetBulkRatingSummaryAsync(
        IEnumerable<Guid> branchIds, CancellationToken ct = default);
}
