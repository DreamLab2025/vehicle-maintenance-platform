using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehiclePartCategoryRepository : IGenericRepository<VehiclePartCategory>
    {
        Task<VehiclePartCategory?> GetByCodeAsync(string code);
    }
}
