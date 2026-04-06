namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageReferralRepository : IGenericRepository<GarageReferral>
{
    Task<bool> ExistsAsync(Guid garageId, Guid userId, CancellationToken ct = default);

    Task<(List<GarageReferral> Items, int TotalCount)> GetPagedByGarageAsync(
        Guid garageId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);

    Task<int> CountByGarageAsync(Guid garageId, CancellationToken ct = default);
}
