using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class DefaultMaintenanceScheduleService : IDefaultMaintenanceScheduleService
    {
        private readonly ILogger<DefaultMaintenanceScheduleService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DefaultMaintenanceScheduleService(
            ILogger<DefaultMaintenanceScheduleService> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<DefaultMaintenanceScheduleResponse>>> GetByVehicleModelIdAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate vehicle model exists
                var vehicleModel = await _unitOfWork.VehicleModels.GetByIdAsync(vehicleModelId);
                if (vehicleModel == null)
                {
                    _logger.LogWarning("Vehicle model not found: {VehicleModelId}", vehicleModelId);
                    return ApiResponse<List<DefaultMaintenanceScheduleResponse>>.FailureResponse(
                        "Không tìm thấy mẫu xe");
                }

                // Get default maintenance schedules for this vehicle model
                var schedules = await _unitOfWork.DefaultMaintenanceSchedules
                    .GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);

                // Filter active schedules and order by display order
                var activeSchedules = schedules
                    .Where(s => s.Status == EntityStatus.Active && s.PartCategory.Status == EntityStatus.Active)
                    .OrderBy(s => s.PartCategory.DisplayOrder)
                    .ToList();

                if (!activeSchedules.Any())
                {
                    _logger.LogInformation("No active maintenance schedules found for vehicle model: {VehicleModelId}", vehicleModelId);
                    return ApiResponse<List<DefaultMaintenanceScheduleResponse>>.SuccessResponse(
                        new List<DefaultMaintenanceScheduleResponse>(),
                        "Chưa có lịch bảo dưỡng mặc định cho mẫu xe này");
                }

                var response = activeSchedules.ToResponseList();

                _logger.LogInformation(
                    "Retrieved {Count} default maintenance schedules for vehicle model: {VehicleModelId}",
                    response.Count,
                    vehicleModelId);

                return ApiResponse<List<DefaultMaintenanceScheduleResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default maintenance schedules for vehicle model: {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<DefaultMaintenanceScheduleResponse>>.FailureResponse(
                    "Có lỗi xảy ra khi lấy lịch bảo dưỡng mặc định");
            }
        }

        public async Task<ApiResponse<DefaultMaintenanceScheduleResponse>> GetByVehicleModelAndPartCategoryAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(partCategoryCode))
                {
                    return ApiResponse<DefaultMaintenanceScheduleResponse>.FailureResponse(
                        "Mã linh kiện không hợp lệ");
                }

                // Validate vehicle model exists
                var vehicleModel = await _unitOfWork.VehicleModels.GetByIdAsync(vehicleModelId);
                if (vehicleModel == null)
                {
                    _logger.LogWarning("Vehicle model not found: {VehicleModelId}", vehicleModelId);
                    return ApiResponse<DefaultMaintenanceScheduleResponse>.FailureResponse(
                        "Không tìm thấy mẫu xe");
                }

                // Get all schedules for this model
                var schedules = await _unitOfWork.DefaultMaintenanceSchedules
                    .GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);

                // Find specific part category (case-insensitive)
                var schedule = schedules
                    .FirstOrDefault(s =>
                        s.PartCategory.Code.Equals(partCategoryCode, StringComparison.OrdinalIgnoreCase) &&
                        s.Status == EntityStatus.Active &&
                        s.PartCategory.Status == EntityStatus.Active);

                if (schedule == null)
                {
                    _logger.LogWarning(
                        "No active schedule found for vehicle model {VehicleModelId} and part category {PartCategoryCode}",
                        vehicleModelId, partCategoryCode);
                    return ApiResponse<DefaultMaintenanceScheduleResponse>.FailureResponse(
                        $"Không tìm thấy lịch bảo dưỡng cho linh kiện '{partCategoryCode}'");
                }

                var response = schedule.ToResponse();

                _logger.LogInformation(
                    "Retrieved schedule for vehicle model {VehicleModelId}, part category {PartCategoryCode}",
                    vehicleModelId, partCategoryCode);

                return ApiResponse<DefaultMaintenanceScheduleResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting schedule for vehicle model {VehicleModelId} and part category {PartCategoryCode}",
                    vehicleModelId, partCategoryCode);
                return ApiResponse<DefaultMaintenanceScheduleResponse>.FailureResponse(
                    "Có lỗi xảy ra khi lấy lịch bảo dưỡng");
            }
        }
    }
}
