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

        /// <summary>
        /// Get default maintenance schedule for a specific part category of a vehicle model
        /// Used by frontend to get schedule for individual part when doing per-part questionnaire
        /// </summary>
        /// <param name="vehicleModelId">Vehicle model ID</param>
        /// <param name="partCategoryCode">Part category code (e.g., "engine_oil", "oil_filter")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Single default maintenance schedule for the specified part</returns>
        Task<ApiResponse<DefaultMaintenanceScheduleResponse>> GetByVehicleModelAndPartCategoryAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default);
    }
}
