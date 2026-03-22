using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartProductRepository : IGenericRepository<PartProduct>
    {
        IQueryable<PartProduct> AsQueryableWithCategory();

        Task<PartProduct?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<PartProduct>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PartProduct>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    }
}
