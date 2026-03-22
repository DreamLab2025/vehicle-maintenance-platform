using MassTransit;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class VariantService(
        ILogger<VariantService> logger,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint) : IVariantService
    {
        private readonly ILogger<VariantService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<ApiResponse<List<VariantResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId)
        {
            var model = await _unitOfWork.Models.GetByIdAsync(vehicleModelId);
            if (model == null)
            {
                _logger.LogWarning("GetImagesByModelId: model not found {VehicleModelId}", vehicleModelId);
                return ApiResponse<List<VariantResponse>>.NotFoundResponse("Không tìm thấy mẫu xe");
            }

            var images = await _unitOfWork.Variants.GetImagesByVehicleModelIdAsync(vehicleModelId);
            var response = images.Select(img => img.ToResponse()).ToList();

            return ApiResponse<List<VariantResponse>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<VariantResponse>> CreateImageAsync(VariantRequest request)
        {
            var model = await _unitOfWork.Models.GetByIdAsync(request.VehicleModelId);
            if (model == null)
            {
                _logger.LogWarning("CreateImage: model not found {VehicleModelId}", request.VehicleModelId);
                return ApiResponse<VariantResponse>.NotFoundResponse("Không tìm thấy mẫu xe");
            }

            var existingImage = await _unitOfWork.Variants.GetImageByVehicleModelIdAndColorAsync(request.VehicleModelId, request.Color);

            if (existingImage != null)
            {
                _logger.LogWarning("CreateImage: color exists {VehicleModelId} {Color}", request.VehicleModelId, request.Color);
                return ApiResponse<VariantResponse>.ConflictResponse("Màu xe này đã tồn tại cho mẫu xe");
            }

            var image = request.ToEntity();
            await _unitOfWork.Variants.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<VariantResponse>.CreatedResponse(
                image.ToResponse(),
                "Tạo hình ảnh xe thành công");
        }

        public async Task<ApiResponse<VariantResponse>> UpdateImageAsync(Guid id, VariantUpdateRequest request)
        {
            var image = await _unitOfWork.Variants.GetByIdAsync(id);
            if (image == null)
            {
                _logger.LogWarning("UpdateImage: not found {ImageId}", id);
                return ApiResponse<VariantResponse>.NotFoundResponse("Không tìm thấy hình ảnh");
            }

            if (image.Color != request.Color)
            {
                var existingImage = await _unitOfWork.Variants.GetImageByVehicleModelIdAndColorAsync(image.VehicleModelId, request.Color);

                if (existingImage != null)
                {
                    _logger.LogWarning("UpdateImage: target color exists {VehicleModelId} {Color}", image.VehicleModelId, request.Color);
                    return ApiResponse<VariantResponse>.ConflictResponse("Màu xe này đã tồn tại cho mẫu xe");
                }
            }

            var previousImageMediaFileId = image.ImageMediaFileId;

            image.UpdateEntity(request);
            await _unitOfWork.Variants.UpdateAsync(image.Id, image);
            await _unitOfWork.SaveChangesAsync();

            if (previousImageMediaFileId.HasValue && previousImageMediaFileId != request.ImageMediaFileId)
            {
                await TryPublishVariantImageSupersededAsync(id, previousImageMediaFileId);
            }

            return ApiResponse<VariantResponse>.SuccessResponse(
                image.ToResponse(),
                "Cập nhật hình ảnh xe thành công");
        }

        public async Task<ApiResponse<string>> DeleteImageAsync(Guid id)
        {
            var image = await _unitOfWork.Variants.GetByIdAsync(id);
            if (image == null)
            {
                _logger.LogWarning("DeleteImage: not found {ImageId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy hình ảnh");
            }

            var supersededImageMediaFileId = image.ImageMediaFileId;

            await _unitOfWork.Variants.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await TryPublishVariantImageSupersededAsync(id, supersededImageMediaFileId);

            return ApiResponse<string>.SuccessResponse("Xóa hình ảnh xe thành công");
        }

        private async Task TryPublishVariantImageSupersededAsync(Guid variantId, Guid? supersededMediaFileId)
        {
            if (!supersededMediaFileId.HasValue || supersededMediaFileId.Value == Guid.Empty)
            {
                return;
            }

            try
            {
                await _publishEndpoint.Publish(new VariantImageMediaSupersededEvent
                {
                    VariantId = variantId,
                    SupersededMediaFileId = supersededMediaFileId.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to publish VariantImageMediaSuperseded for variant {VariantId}, media {MediaFileId}",
                    variantId, supersededMediaFileId);
            }
        }
    }
}
