using Microsoft.Extensions.Logging;
using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;
using Verender.Vehicle.Application.Mappings;
using Verender.Vehicle.Application.Services.Interfaces;
using Verender.Vehicle.Domain.Repositories.Interfaces;

namespace Verender.Vehicle.Application.Services.Implements
{
    public class VehicleVariantService : IVehicleVariantService
    {
        private readonly ILogger<VehicleVariantService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleVariantService(ILogger<VehicleVariantService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<VehicleVariantResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(vehicleModelId);
                if (model == null)
                {
                    return ApiResponse<List<VehicleVariantResponse>>.FailureResponse("Không tìm thấy mẫu xe");
                }

                var images = await _unitOfWork.VehicleVariants.GetImagesByVehicleModelIdAsync(vehicleModelId);
                var response = images.Select(img => img.ToResponse()).ToList();

                return ApiResponse<List<VehicleVariantResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for vehicle model {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<VehicleVariantResponse>>.FailureResponse("Có lỗi xảy ra khi lấy danh sách hình ảnh");
            }
        }
        public async Task<ApiResponse<VehicleVariantResponse>> CreateImageAsync(VehicleVariantRequest request)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(request.VehicleModelId);
                if (model == null)
                {
                    return ApiResponse<VehicleVariantResponse>.FailureResponse("Không tìm thấy mẫu xe");
                }

                var existingImage = await _unitOfWork.VehicleVariants.GetImageByVehicleModelIdAndColorAsync(request.VehicleModelId, request.Color);

                if (existingImage != null)
                {
                    return ApiResponse<VehicleVariantResponse>.FailureResponse("Màu xe này đã tồn tại cho mẫu xe");
                }

                var image = request.ToEntity();
                await _unitOfWork.VehicleVariants.AddAsync(image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created model image for model {VehicleModelId} with color {Color}",
                    request.VehicleModelId, request.Color);

                return ApiResponse<VehicleVariantResponse>.SuccessResponse(
                    image.ToResponse(),
                    "Tạo hình ảnh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model image for model {VehicleModelId}", request.VehicleModelId);
                return ApiResponse<VehicleVariantResponse>.FailureResponse("Có lỗi xảy ra khi tạo hình ảnh");
            }
        }

        public async Task<ApiResponse<VehicleVariantResponse>> UpdateImageAsync(Guid id, VehicleVariantUpdateRequest request)
        {
            try
            {
                var image = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<VehicleVariantResponse>.FailureResponse("Không tìm thấy hình ảnh");
                }

                if (image.Color != request.Color)
                {
                    var existingImage = await _unitOfWork.VehicleVariants.GetImageByVehicleModelIdAndColorAsync(image.VehicleModelId, request.Color);

                    if (existingImage != null)
                    {
                        return ApiResponse<VehicleVariantResponse>.FailureResponse("Màu xe này đã tồn tại cho mẫu xe");
                    }
                }

                image.UpdateEntity(request);
                await _unitOfWork.VehicleVariants.UpdateAsync(image.Id, image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated model image {ImageId}", id);

                return ApiResponse<VehicleVariantResponse>.SuccessResponse(
                    image.ToResponse(),
                    "Cập nhật hình ảnh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model image {ImageId}", id);
                return ApiResponse<VehicleVariantResponse>.FailureResponse("Có lỗi xảy ra khi cập nhật hình ảnh");
            }
        }

        public async Task<ApiResponse<string>> DeleteImageAsync(Guid id)
        {
            try
            {
                var image = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy hình ảnh");
                }

                await _unitOfWork.VehicleVariants.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted model image {ImageId}", id);

                return ApiResponse<string>.SuccessResponse("Xóa hình ảnh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model image {ImageId}", id);
                return ApiResponse<string>.FailureResponse("Có lỗi xảy ra khi xóa hình ảnh");
            }
        }
    }
}
