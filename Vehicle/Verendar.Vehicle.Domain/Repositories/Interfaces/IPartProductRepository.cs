using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartProductRepository : IGenericRepository<PartProduct>
    {
        Task<IEnumerable<PartProduct>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);
    }
}
