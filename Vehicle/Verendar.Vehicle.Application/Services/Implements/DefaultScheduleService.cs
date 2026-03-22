using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class DefaultScheduleService(
        ILogger<DefaultScheduleService> logger,
        IUnitOfWork unitOfWork) : IDefaultScheduleService
    {
        private readonly ILogger<DefaultScheduleService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<PartCategoryResponse>>> GetPartCategoriesByVehicleModelAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default)
        {
            var vehicleModel = await _unitOfWork.Models.GetByIdAsync(vehicleModelId);
            if (vehicleModel == null)
            {
                _logger.LogWarning("GetPartCategoriesByVehicleModel: model not found {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<PartCategoryResponse>>.NotFoundResponse("Không tìm thấy mẫu xe");
            }

            var schedules = await _unitOfWork.DefaultSchedules.GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);
            var categories = schedules
                .Where(s => s.PartCategory != null)
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

        public async Task<ApiResponse<DefaultScheduleResponse>> GetDefaultScheduleByModelAndPartCategorySlugAsync(
            Guid vehicleModelId,
            string partCategorySlug,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(partCategorySlug))
            {
                _logger.LogWarning("GetDefaultScheduleByModelAndPartCategorySlug: empty slug model {VehicleModelId}", vehicleModelId);
                return ApiResponse<DefaultScheduleResponse>.FailureResponse("Slug danh mục phụ tùng không hợp lệ");
            }

            var schedule = await _unitOfWork.DefaultSchedules.GetByVehicleModelIdAndPartCategorySlugAsync(
                vehicleModelId,
                partCategorySlug,
                cancellationToken);

            if (schedule == null)
            {
                _logger.LogWarning(
                    "GetDefaultScheduleByModelAndPartCategorySlug: not found model {VehicleModelId} slug {PartCategorySlug}",
                    vehicleModelId,
                    partCategorySlug);
                return ApiResponse<DefaultScheduleResponse>.NotFoundResponse(
                    "Không tìm thấy lịch bảo dưỡng mặc định cho mẫu xe và danh mục này");
            }

            return ApiResponse<DefaultScheduleResponse>.SuccessResponse(
                schedule.ToResponse(),
                "Lấy lịch bảo dưỡng mặc định thành công");
        }
    }
}
