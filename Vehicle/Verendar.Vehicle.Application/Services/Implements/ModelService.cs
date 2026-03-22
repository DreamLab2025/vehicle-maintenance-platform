using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class ModelService(ILogger<ModelService> logger, IUnitOfWork unitOfWork) : IModelService
    {
        private readonly ILogger<ModelService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<ModelResponseWithVariants>> CreateModelAsync(ModelRequest request)
        {
            var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
            if (!isValid)
            {
                _logger.LogWarning("CreateModel: invalid type/brand — {Message}", errorMessage);
                return ApiResponse<ModelResponseWithVariants>.NotFoundResponse(errorMessage!);
            }

            if (await ModelNameExistsAsync(request.Name, request.BrandId))
            {
                _logger.LogWarning("CreateModel: duplicate name {ModelName} for brand {BrandId}", request.Name, request.BrandId);
                return ApiResponse<ModelResponseWithVariants>.ConflictResponse("Mẫu xe đã tồn tại cho thương hiệu này");
            }

            var model = request.ToEntity();

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.Models.AddAsync(model);

                if (request.Images != null && request.Images.Any())
                {
                    foreach (var imageItem in request.Images)
                    {
                        var image = new Variant
                        {
                            VehicleModelId = model.Id,
                            Color = imageItem.Color,
                            HexCode = ColorCode.IsHex(imageItem.HexCode) ? imageItem.HexCode : "#000000",
                            ImageUrl = imageItem.ImageUrl
                        };
                        await _unitOfWork.Variants.AddAsync(image);
                    }
                }
            });

            var createdModel = await GetModelWithDetailsAsync(model.Id);

            return ApiResponse<ModelResponseWithVariants>.CreatedResponse(
                createdModel!.ToModelResponseWithVariants(),
                "Tạo mẫu xe thành công");
        }

        public async Task<ApiResponse<string>> DeleteModelAsync(Guid id)
        {
            var model = await _unitOfWork.Models.GetByIdAsync(id);

            if (model == null)
            {
                _logger.LogWarning("DeleteModel: not found {ModelId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy mẫu xe");
            }

            await _unitOfWork.Models.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(
                "Deleted",
                "Xóa mẫu xe thành công");
        }

        public async Task<ApiResponse<List<ModelSummary>>> GetAllModelsAsync(ModelFilterRequest filterRequest)
        {
            filterRequest.Normalize();
            var query = _unitOfWork.Models.AsQueryableWithBrandAndVehicleType();

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

            var modelSummaries = items.Select(m => m.ToModelSummary()).ToList();

            return ApiResponse<List<ModelSummary>>.SuccessPagedResponse(
                modelSummaries,
                totalCount,
                filterRequest.PageNumber,
                filterRequest.PageSize,
                "Lấy danh sách mẫu xe thành công");
        }

        public async Task<ApiResponse<ModelResponse>> UpdateModelAsync(Guid id, ModelRequest request)
        {
            var model = await _unitOfWork.Models.GetByIdAsync(id);

            if (model == null)
            {
                _logger.LogWarning("UpdateModel: not found {ModelId}", id);
                return ApiResponse<ModelResponse>.NotFoundResponse("Không tìm thấy mẫu xe");
            }

            var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
            if (!isValid)
            {
                _logger.LogWarning("UpdateModel: invalid type/brand for {ModelId} — {Message}", id, errorMessage);
                return ApiResponse<ModelResponse>.NotFoundResponse(errorMessage!);
            }

            if (await ModelNameExistsAsync(request.Name, request.BrandId, id))
            {
                _logger.LogWarning("UpdateModel: name conflict {ModelName} brand {BrandId}", request.Name, request.BrandId);
                return ApiResponse<ModelResponse>.ConflictResponse("Tên mẫu xe đã tồn tại cho thương hiệu này");
            }

            model.UpdateEntity(request);
            await _unitOfWork.Models.UpdateAsync(id, model);
            await _unitOfWork.SaveChangesAsync();

            var updatedModel = await GetModelWithDetailsAsync(id);

            return ApiResponse<ModelResponse>.SuccessResponse(
                updatedModel!.ToModelResponse(),
                "Cập nhật mẫu xe thành công");
        }

        public async Task<ApiResponse<ModelResponseWithVariants>> GetModelByIdAsync(Guid id)
        {
            var model = await GetModelWithDetailsAsync(id);
            if (model == null)
            {
                _logger.LogWarning("GetModelById: not found {ModelId}", id);
                return ApiResponse<ModelResponseWithVariants>.NotFoundResponse("Không tìm thấy mẫu xe");
            }
            return ApiResponse<ModelResponseWithVariants>.SuccessResponse(
                model.ToModelResponseWithVariants(),
                "Lấy mẫu xe thành công");
        }

        private async Task<(bool IsValid, Brand? Brand, VehicleType? Type, string? ErrorMessage)> ValidateTypeBrandRelationshipAsync(Guid typeId, Guid brandId)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
            if (brand == null)
            {
                return (false, null, null, "Thương hiệu không tồn tại");
            }

            var type = await _unitOfWork.Types.GetByIdAsync(typeId);
            if (type == null)
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
            var existingModel = await _unitOfWork.Models
                .FindOneAsync(m => m.Name.ToLower() == nameLower
                    && m.VehicleBrandId == brandId);

            return existingModel != null && (!excludeId.HasValue || existingModel.Id != excludeId.Value);
        }

        private Task<Model?> GetModelWithDetailsAsync(Guid id) =>
            _unitOfWork.Models.GetByIdWithBrandTypeAndVariantsAsync(id);
    }
}
