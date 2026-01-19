using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IVehiclePartCategoryRepository : IGenericRepository<VehiclePartCategory>
    {
        Task<VehiclePartCategory?> GetByCodeAsync(string code);
    }
}
