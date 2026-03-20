using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartCategoryRepository : IGenericRepository<PartCategory>
    {
        Task<PartCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCategory>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);
    }
}
