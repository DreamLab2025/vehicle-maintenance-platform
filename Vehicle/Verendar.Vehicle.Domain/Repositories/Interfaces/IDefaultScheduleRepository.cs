using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IDefaultScheduleRepository : IGenericRepository<DefaultMaintenanceSchedule>
    {
        Task<IEnumerable<DefaultMaintenanceSchedule>> GetByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
    }
}
