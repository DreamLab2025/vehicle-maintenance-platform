using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IModelRepository : IGenericRepository<Model>
    {
        IQueryable<Model> AsQueryableWithBrandAndVehicleType();

        Task<Model?> GetByIdWithBrandTypeAndVariantsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
