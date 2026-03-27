using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageProductRepository : IGenericRepository<GarageProduct>
{
    Task<GarageProduct?> GetByIdWithInstallationAsync(Guid id, CancellationToken ct = default);

    Task<(List<GarageProduct> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}
