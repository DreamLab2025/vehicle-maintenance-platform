using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartCategoryRepository : IGenericRepository<PartCategory>
    {
        Task<PartCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCategory>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PartCategory>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default);
    }
}
