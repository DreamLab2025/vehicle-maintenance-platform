using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IModelRepository : IGenericRepository<Model>
    {
        /// <summary>Eager-loads brand and type for filtered model listings.</summary>
        IQueryable<Model> AsQueryableWithBrandAndVehicleType();

        Task<Model?> GetByIdWithBrandTypeAndVariantsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
