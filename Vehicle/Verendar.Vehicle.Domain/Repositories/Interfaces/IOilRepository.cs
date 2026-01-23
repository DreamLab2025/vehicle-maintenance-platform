using Verendar.Common.Databases.Interfaces;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOilRepository : IGenericRepository<Oil>
    {
        Task<Oil?> GetByVehiclePartIdAsync(Guid vehiclePartId);
        Task<List<Oil>> GetByVehicleUsageAsync(OilVehicleUsage vehicleUsage);
        Task<List<Oil>> GetByViscosityGradeAsync(string viscosityGrade);
    }
}
