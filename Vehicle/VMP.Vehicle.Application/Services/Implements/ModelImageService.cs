using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Mappings;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Repositories.Interfaces;

namespace VMP.Vehicle.Application.Services.Implements
{
    public class ModelImageService : IModelImageService
    {
        private readonly ILogger<ModelImageService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ModelImageService(ILogger<ModelImageService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<ModelImageResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(vehicleModelId);
                if (model == null)
                {
                    return ApiResponse<List<ModelImageResponse>>.FailureResponse("Không tìm th?y m?u xe");
                }

                var images = await _unitOfWork.ModelImages.GetImagesByVehicleModelIdAsync(vehicleModelId);
                var response = images.Select(img => img.ToResponse()).ToList();

                return ApiResponse<List<ModelImageResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for vehicle model {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<ModelImageResponse>>.FailureResponse("Có l?i x?y ra khi l?y danh sách hình ?nh");
            }
        }

        public async Task<ApiResponse<ModelImageResponse>> GetImageByModelAndColorAsync(Guid vehicleModelId, string color)
        {
            try
            {
                var image = await _unitOfWork.ModelImages.GetImageByVehicleModelIdAndColorAsync(vehicleModelId, color);
                if (image == null)
                {
                    return ApiResponse<ModelImageResponse>.FailureResponse("Không tìm th?y hình ?nh cho màu này");
                }

                return ApiResponse<ModelImageResponse>.SuccessResponse(image.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image for vehicle model {VehicleModelId} and color {Color}", vehicleModelId, color);
                return ApiResponse<ModelImageResponse>.FailureResponse("Có l?i x?y ra khi l?y hình ?nh");
            }
        }

        public async Task<ApiResponse<ModelImageResponse>> CreateImageAsync(ModelImageRequest request)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(request.VehicleModelId);
                if (model == null)
                {
                    return ApiResponse<ModelImageResponse>.FailureResponse("Không tìm th?y m?u xe");
                }

                var existingImage = await _unitOfWork.ModelImages
                    .GetImageByVehicleModelIdAndColorAsync(request.VehicleModelId, request.Color);
                
                if (existingImage != null)
                {
                    return ApiResponse<ModelImageResponse>.FailureResponse("Màu xe này ?ã t?n t?i cho m?u xe");
                }

                var image = request.ToEntity();
                await _unitOfWork.ModelImages.AddAsync(image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created model image for model {VehicleModelId} with color {Color}", 
                    request.VehicleModelId, request.Color);

                return ApiResponse<ModelImageResponse>.SuccessResponse(
                    image.ToResponse(), 
                    "T?o hình ?nh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model image for model {VehicleModelId}", request.VehicleModelId);
                return ApiResponse<ModelImageResponse>.FailureResponse("Có l?i x?y ra khi t?o hình ?nh");
            }
        }

        public async Task<ApiResponse<ModelImageResponse>> UpdateImageAsync(Guid id, ModelImageUpdateRequest request)
        {
            try
            {
                var image = await _unitOfWork.ModelImages.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<ModelImageResponse>.FailureResponse("Không tìm th?y hình ?nh");
                }

                if (image.Color != request.Color)
                {
                    var existingImage = await _unitOfWork.ModelImages
                        .GetImageByVehicleModelIdAndColorAsync(image.VehicleModelId, request.Color);
                    
                    if (existingImage != null)
                    {
                        return ApiResponse<ModelImageResponse>.FailureResponse("Màu xe này ?ã t?n t?i cho m?u xe");
                    }
                }

                image.UpdateEntity(request);
                await _unitOfWork.ModelImages.UpdateAsync(image.Id, image);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated model image {ImageId}", id);

                return ApiResponse<ModelImageResponse>.SuccessResponse(
                    image.ToResponse(),
                    "C?p nh?t hình ?nh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model image {ImageId}", id);
                return ApiResponse<ModelImageResponse>.FailureResponse("Có l?i x?y ra khi c?p nh?t hình ?nh");
            }
        }

        public async Task<ApiResponse<string>> DeleteImageAsync(Guid id)
        {
            try
            {
                var image = await _unitOfWork.ModelImages.GetByIdAsync(id);
                if (image == null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm th?y hình ?nh");
                }

                await _unitOfWork.ModelImages.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted model image {ImageId}", id);

                return ApiResponse<string>.SuccessResponse("Xóa hình ?nh xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model image {ImageId}", id);
                return ApiResponse<string>.FailureResponse("Có l?i x?y ra khi xóa hình ?nh");
            }
        }
    }
}
