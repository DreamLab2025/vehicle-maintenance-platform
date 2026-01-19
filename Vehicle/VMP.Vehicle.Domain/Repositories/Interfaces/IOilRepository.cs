using VMP.Common.Databases.Interfaces;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Domain.Repositories.Interfaces
{
    public interface IOilRepository : IGenericRepository<Oil>
    {
        Task<Oil?> GetByVehiclePartIdAsync(Guid vehiclePartId);
        Task<List<Oil>> GetByVehicleUsageAsync(OilVehicleUsage vehicleUsage);
        Task<List<Oil>> GetByViscosityGradeAsync(string viscosityGrade);
    }
}
