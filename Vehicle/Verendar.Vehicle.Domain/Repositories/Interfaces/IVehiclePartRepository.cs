using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehiclePartRepository : IGenericRepository<VehiclePart>
    {
        Task<List<VehiclePart>> GetByCategoryIdAsync(Guid categoryId);
        Task<List<VehiclePart>> GetByCategoryCodeAsync(string categoryCode);
    }
}
