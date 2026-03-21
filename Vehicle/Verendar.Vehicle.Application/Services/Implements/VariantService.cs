using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class VariantService(ILogger<VariantService> logger, IUnitOfWork unitOfWork) : IVariantService
    {
        private readonly ILogger<VariantService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<VariantResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId)
        {
            try
            {
                var model = await _unitOfWork.Models.GetByIdAsync(vehicleModelId);
                if (model == null)
                {
                    return ApiResponse<List<VariantResponse>>.NotFoundResponse("Không tìm thấy mẫu xe");
                }

                var images = await _unitOfWork.Variants.GetImagesByVehicleModelIdAsync(vehicleModelId);
                var response = images.Select(img => img.ToResponse()).ToList();

                return ApiResponse<List<VariantResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for vehicle model {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<VariantResponse>>.FailureResponse("Có lỗi xảy ra khi lấy danh sách hình ảnh");
            }
        }
        public async Task<ApiResponse<VariantResponse>> CreateImageAsync(VariantRequest request)
        {
            try
            {
                var model = await _unitOfWork.Models.GetByIdAsync(request.VehicleModelId);
                if (model == null)
                {
                    return ApiResponse<VariantResponse>.NotFoundResponse("Không tìm thấy mẫu xe");
                }

                var existingImage = await _unitOfWork.Variants.GetImageByVehicleModelIdAndColorAsync(request.VehicleModelId, request.Color);

                if (existingImage != null)
                {
                    return ApiResponse<VariantResponse>.ConflictResponse("Màu xe này đã tồn tại cho mẫu xe");
                }

                var image = request.ToEntity();
                await _unitOfWork.Variants.AddAsync(image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created model image for model {VehicleModelId} with color {Color}",
                    request.VehicleModelId, request.Color);

                return ApiResponse<VariantResponse>.CreatedResponse(
                    image.ToResponse(),
                    "Tạo hình ảnh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model image for model {VehicleModelId}", request.VehicleModelId);
                return ApiResponse<VariantResponse>.FailureResponse("Có lỗi xảy ra khi tạo hình ảnh");
            }
        }

        public async Task<ApiResponse<VariantResponse>> UpdateImageAsync(Guid id, VariantUpdateRequest request)
        {
            try
            {
                var image = await _unitOfWork.Variants.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<VariantResponse>.NotFoundResponse("Không tìm thấy hình ảnh");
                }

                if (image.Color != request.Color)
                {
                    var existingImage = await _unitOfWork.Variants.GetImageByVehicleModelIdAndColorAsync(image.VehicleModelId, request.Color);

                    if (existingImage != null)
                    {
                        return ApiResponse<VariantResponse>.ConflictResponse("Màu xe này đã tồn tại cho mẫu xe");
                    }
                }

                image.UpdateEntity(request);
                await _unitOfWork.Variants.UpdateAsync(image.Id, image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated model image {ImageId}", id);

                return ApiResponse<VariantResponse>.SuccessResponse(
                    image.ToResponse(),
                    "Cập nhật hình ảnh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model image {ImageId}", id);
                return ApiResponse<VariantResponse>.FailureResponse("Có lỗi xảy ra khi cập nhật hình ảnh");
            }
        }

        public async Task<ApiResponse<string>> DeleteImageAsync(Guid id)
        {
            try
            {
                var image = await _unitOfWork.Variants.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy hình ảnh");
                }

                await _unitOfWork.Variants.DeleteAsync(id);
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
