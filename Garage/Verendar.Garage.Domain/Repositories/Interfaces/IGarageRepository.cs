using System.Linq.Expressions;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageRepository : IGenericRepository<global::Verendar.Garage.Domain.Entities.Garage>
{
    Task<global::Verendar.Garage.Domain.Entities.Garage?> GetWithBranchesAsync(
        Expression<Func<global::Verendar.Garage.Domain.Entities.Garage, bool>> filter,
        CancellationToken ct = default);
}
