using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IServiceCategoryRepository : IGenericRepository<ServiceCategory>
{
    Task<ServiceCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<List<ServiceCategory>> GetAllOrderedAsync(CancellationToken ct = default);
}
