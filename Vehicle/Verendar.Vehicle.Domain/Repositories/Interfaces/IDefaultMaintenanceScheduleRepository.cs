using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IDefaultMaintenanceScheduleRepository : IGenericRepository<DefaultMaintenanceSchedule>
    {
        Task<IEnumerable<DefaultMaintenanceSchedule>> GetByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
        Task<DefaultMaintenanceSchedule?> GetByVehicleModelAndPartCategoryAsync(Guid vehicleModelId, Guid partCategoryId, CancellationToken cancellationToken = default);
    }
}
