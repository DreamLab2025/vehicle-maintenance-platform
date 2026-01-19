using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehiclePartRepository : IGenericRepository<VehiclePart>
    {
        Task<List<VehiclePart>> GetByCategoryIdAsync(Guid categoryId);
        Task<List<VehiclePart>> GetByCategoryCodeAsync(string categoryCode);
    }
}
