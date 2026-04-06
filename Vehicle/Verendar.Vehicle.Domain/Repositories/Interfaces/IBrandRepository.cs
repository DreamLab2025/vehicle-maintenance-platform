using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        IQueryable<Brand> AsQueryableWithVehicleType();

        Task<Brand?> GetByIdWithTypesAsync(Guid id);
    }
}
