using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class VehicleModelService(ILogger<VehicleModelService> logger, IUnitOfWork unitOfWork) : IVehicleModelService
    {
        private readonly ILogger<VehicleModelService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<ModelResponseWithVariants>> CreateModelAsync(ModelRequest request)
        {
            try
            {
                var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
                if (!isValid)
                {
                    return ApiResponse<ModelResponseWithVariants>.FailureResponse(errorMessage!);
                }

                if (await ModelNameExistsAsync(request.Name, request.BrandId))
                {
                    return ApiResponse<ModelResponseWithVariants>.FailureResponse("Mẫu xe đã tồn tại cho thương hiệu này");
                }

                var model = request.ToEntity();
                await _unitOfWork.VehicleModels.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                // Add images if provided
                if (request.Images != null && request.Images.Any())
                {
                    foreach (var imageItem in request.Images)
                    {
                        var image = new VehicleVariant
                        {
                            VehicleModelId = model.Id,
                            Color = imageItem.Color,
                            HexCode = ColorCode.IsHex(imageItem.HexCode) ? imageItem.HexCode : "#000000",
                            ImageUrl = imageItem.ImageUrl
                        };
                        await _unitOfWork.VehicleVariants.AddAsync(image);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                var createdModel = await GetModelWithDetailsAsync(model.Id);

                _logger.LogInformation("Created vehicle model {ModelName} (ID: {ModelId}) for brand {BrandName} with {ImageCount} images",
                    model.Name, model.Id, brand!.Name, request.Images?.Count ?? 0);

                return ApiResponse<ModelResponseWithVariants>.SuccessResponse(
                    createdModel!.ToModelResponseWithVariants(),
                    "Tạo mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle model");
                return ApiResponse<ModelResponseWithVariants>.FailureResponse("Lỗi khi tạo mẫu xe");
            }
        }

        public async Task<ApiResponse<string>> DeleteModelAsync(Guid id)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(id);

                if (model == null || model.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy mẫu xe");
                }

                await _unitOfWork.VehicleModels.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted vehicle model with ID: {ModelId}", id);

                return ApiResponse<string>.SuccessResponse(
                    "Deleted",
                    "Xóa mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle model with ID: {ModelId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa mẫu xe");
            }
        }

        public async Task<ApiResponse<List<ModelResponseWithVariants>>> GetAllModelsAsync(ModelFilterRequest filterRequest)
        {
            try
            {
                filterRequest.Normalize();
                var query = _unitOfWork.VehicleModels.AsQueryable()
                    .Include(m => m.Brand)
                    .Include(m => m.Brand).ThenInclude(b => b.VehicleType)
                    .Include(m => m.Variants)
                    .Where(m => m.DeletedAt == null);

                // Apply filters by ID (more efficient than name search)
                if (filterRequest.TypeId.HasValue)
                {
                    query = query.Where(m => m.Brand.VehicleTypeId == filterRequest.TypeId.Value);
                }

                if (filterRequest.BrandId.HasValue)
                {
                    query = query.Where(m => m.VehicleBrandId == filterRequest.BrandId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filterRequest.ModelName))
                {
                    var searchLower = filterRequest.ModelName.ToLower();
                    query = query.Where(m => m.Name.ToLower().Contains(searchLower));
                }

                if (filterRequest.TransmissionType.HasValue)
                {
                    query = query.Where(m => m.TransmissionType == filterRequest.TransmissionType.Value);
                }

                if (filterRequest.EngineDisplacement.HasValue)
                {
                    query = query.Where(m => m.EngineDisplacement == filterRequest.EngineDisplacement.Value);
                }

                if (filterRequest.ReleaseYear.HasValue)
                {
                    query = query.Where(m => m.ManufactureYear == filterRequest.ReleaseYear.Value);
                }

                var totalCount = await query.CountAsync();

                if (filterRequest.IsDescending.HasValue)
                {
                    query = filterRequest.IsDescending.Value
                        ? query.OrderByDescending(m => m.CreatedAt)
                        : query.OrderBy(m => m.CreatedAt);
                }

                var items = await query
                    .Skip((filterRequest.PageNumber - 1) * filterRequest.PageSize)
                    .Take(filterRequest.PageSize)
                    .ToListAsync();

                var modelResponses = items.Select(m => m.ToModelResponseWithVariants()).ToList();

                return ApiResponse<ModelResponseWithVariants>.SuccessPagedResponse(
                    modelResponses,
                    totalCount,
                    filterRequest.PageNumber,
                    filterRequest.PageSize,
                    "Lấy danh sách mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicle models");
                return ApiResponse<List<ModelResponseWithVariants>>.FailureResponse("Lỗi khi lấy danh sách mẫu xe");
            }
        }

        public async Task<ApiResponse<ModelResponse>> UpdateModelAsync(Guid id, ModelRequest request)
        {
            try
            {
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(id);

                if (model == null || model.DeletedAt != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Không tìm thấy mẫu xe");
                }

                var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
                if (!isValid)
                {
                    return ApiResponse<ModelResponse>.FailureResponse(errorMessage!);
                }

                if (await ModelNameExistsAsync(request.Name, request.BrandId, id))
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Tên mẫu xe đã tồn tại cho thương hiệu này");
                }

                model.UpdateEntity(request);
                await _unitOfWork.VehicleModels.UpdateAsync(id, model);
                await _unitOfWork.SaveChangesAsync();

                var updatedModel = await GetModelWithDetailsAsync(id);

                _logger.LogInformation("Updated vehicle model {ModelName} (ID: {ModelId})", model.Name, id);

                return ApiResponse<ModelResponse>.SuccessResponse(
                    updatedModel!.ToModelResponse(),
                    "Cập nhật mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle model with ID: {ModelId}", id);
                return ApiResponse<ModelResponse>.FailureResponse("Lỗi khi cập nhật mẫu xe");
            }
        }

        public async Task<ApiResponse<ModelResponseWithVariants>> GetModelByIdAsync(Guid id)
        {
            try
            {
                var model = await GetModelWithDetailsAsync(id);
                if (model == null || model.DeletedAt != null)
                {
                    return ApiResponse<ModelResponseWithVariants>.FailureResponse("Không tìm thấy mẫu xe");
                }
                return ApiResponse<ModelResponseWithVariants>.SuccessResponse(
                    model.ToModelResponseWithVariants(),
                    "Lấy mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle model with ID: {ModelId}", id);
                return ApiResponse<ModelResponseWithVariants>.FailureResponse("Lỗi khi lấy mẫu xe");
            }
        }

        private async Task<(bool IsValid, VehicleBrand? Brand, VehicleType? Type, string? ErrorMessage)> ValidateTypeBrandRelationshipAsync(Guid typeId, Guid brandId)
        {
            var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(brandId);
            if (brand == null || brand.DeletedAt != null)
            {
                return (false, null, null, "Thương hiệu không tồn tại");
            }

            var type = await _unitOfWork.VehicleTypes.GetByIdAsync(typeId);
            if (type == null || type.DeletedAt != null)
            {
                return (false, null, null, "Loại xe không tồn tại");
            }

            if (brand.VehicleTypeId != typeId)
            {
                return (false, null, null,
                    $"Thương hiệu '{brand.Name}' không hỗ trợ loại xe '{type.Name}'. Vui lòng chọn thương hiệu phù hợp với loại xe.");
            }

            return (true, brand, type, null);
        }

        private async Task<bool> ModelNameExistsAsync(string name, Guid brandId, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var nameLower = name.Trim().ToLower();
            var existingModel = await _unitOfWork.VehicleModels
                .FindOneAsync(m => m.Name.ToLower() == nameLower
                    && m.VehicleBrandId == brandId
                    && m.DeletedAt == null);

            return existingModel != null && (!excludeId.HasValue || existingModel.Id != excludeId.Value);
        }

        private async Task<VehicleModel?> GetModelWithDetailsAsync(Guid id)
        {
            return await _unitOfWork.VehicleModels.AsQueryable()
                .Include(m => m.Brand)
                .Include(m => m.Brand).ThenInclude(b => b.VehicleType)
                .Include(m => m.Variants)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
