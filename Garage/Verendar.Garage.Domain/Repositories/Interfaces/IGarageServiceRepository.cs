using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageServiceRepository : IGenericRepository<GarageService>
{
    Task<GarageService?> GetByIdWithCategoryAsync(Guid id, CancellationToken ct = default);

    Task<GarageService?> GetByIdWithCategoryForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<(List<GarageService> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        string? name = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        Guid? categoryId = null,
        CancellationToken ct = default);
}
