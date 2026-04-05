using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageBundleRepository : IGenericRepository<GarageBundle>
{
    Task<GarageBundle?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);

    Task<GarageBundle?> GetByIdWithItemsForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<(List<GarageBundle> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}
