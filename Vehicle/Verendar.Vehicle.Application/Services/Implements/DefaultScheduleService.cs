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

    }
}
