using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IDefaultMaintenanceScheduleService
    {
        Task<ApiResponse<DefaultMaintenanceScheduleResponse>> GetByVehicleModelAndPartCategoryAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default);
    }
}
