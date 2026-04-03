using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageBranchRepository : IGenericRepository<GarageBranch>
{
    Task<Guid?> GetGarageOwnerIdByBranchIdAsync(Guid branchId, CancellationToken ct = default);

    Task<(List<GarageBranch> Items, int TotalCount)> GetBranchesForMapAsync(
        int pageNumber,
        int pageSize,
        double? minLat = null,
        double? maxLat = null,
        double? minLng = null,
        double? maxLng = null,
        CancellationToken ct = default);
}
