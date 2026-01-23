using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IPartProductRepository : IGenericRepository<PartProduct>
    {
        Task<IEnumerable<PartProduct>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<PartProduct?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);
    }
}
