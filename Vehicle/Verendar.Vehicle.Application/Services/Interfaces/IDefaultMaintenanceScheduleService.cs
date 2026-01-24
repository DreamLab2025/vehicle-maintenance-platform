using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IDefaultMaintenanceScheduleService
    {
        /// <summary>
        /// Get all default maintenance schedules for a vehicle model
        /// Used by frontend to display manufacturer's recommended maintenance
        /// and by AI service for analysis
        /// </summary>
        /// <param name="vehicleModelId">Vehicle model ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of default maintenance schedules ordered by display order</returns>
        Task<ApiResponse<List<DefaultMaintenanceScheduleResponse>>> GetByVehicleModelIdAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default);
    }
}
