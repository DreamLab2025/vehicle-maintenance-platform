using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        /// <summary>Eager-loads <see cref="Brand.VehicleType"/> for listing/filter queries.</summary>
        IQueryable<Brand> AsQueryableWithVehicleType();

        Task<Brand?> GetByIdWithTypesAsync(Guid id);
    }
}
