using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class DefaultMaintenanceScheduleService(
        ILogger<DefaultMaintenanceScheduleService> logger,
        IUnitOfWork unitOfWork) : IDefaultMaintenanceScheduleService
    {
        private readonly ILogger<DefaultMaintenanceScheduleService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<PartCategoryResponse>>> GetPartCategoriesByVehicleModelAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var vehicleModel = await _unitOfWork.VehicleModels.GetByIdAsync(vehicleModelId);
                if (vehicleModel == null)
                {
                    _logger.LogWarning("Vehicle model not found: {VehicleModelId}", vehicleModelId);
                    return ApiResponse<List<PartCategoryResponse>>.NotFoundResponse("Không tìm thấy mẫu xe");
                }

                var schedules = await _unitOfWork.DefaultMaintenanceSchedules.GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);
                var categories = schedules
                    .Where(s => s.Status == EntityStatus.Active && s.PartCategory != null && s.PartCategory.Status == EntityStatus.Active)
                    .Select(s => s.PartCategory!)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.CreatedAt)
                    .Select(c => c.ToResponse())
                    .ToList();

                return ApiResponse<List<PartCategoryResponse>>.SuccessResponse(
                    categories,
                    "Lấy danh mục linh kiện áp dụng cho mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part categories for vehicle model {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<PartCategoryResponse>>.FailureResponse("Có lỗi xảy ra khi lấy danh mục linh kiện theo mẫu xe");
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
                    return ApiResponse<DefaultMaintenanceScheduleResponse>.NotFoundResponse(
                        "Không tìm thấy mẫu xe");
                }

                // Get all schedules for this model
                var schedules = await _unitOfWork.DefaultMaintenanceSchedules.GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);

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
                    return ApiResponse<DefaultMaintenanceScheduleResponse>.NotFoundResponse(
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
