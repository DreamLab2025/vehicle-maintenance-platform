using System.Linq.Expressions;
using Verendar.Garage.Domain.Models;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageRepository : IGenericRepository<global::Verendar.Garage.Domain.Entities.Garage>
{
    Task<global::Verendar.Garage.Domain.Entities.Garage?> GetWithBranchesAsync(
        Expression<Func<global::Verendar.Garage.Domain.Entities.Garage, bool>> filter,
        CancellationToken ct = default);

    Task<GarageStatusCounts> GetStatusCountsAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);
}
