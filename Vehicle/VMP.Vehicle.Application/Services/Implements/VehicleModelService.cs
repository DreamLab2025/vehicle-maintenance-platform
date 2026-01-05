using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Mappings;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;

namespace VMP.Vehicle.Application.Services.Implements
{
    public class VehicleModelService : IVehicleModelService
    {
        private readonly ILogger<VehicleModelService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleModelService(ILogger<VehicleModelService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ModelResponse>> CreateModelAsync(ModelRequest request)
        {
            try
            {
                var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
                if (!isValid)
                {
                    return ApiResponse<ModelResponse>.FailureResponse(errorMessage!);
                }

                if (await ModelNameExistsAsync(request.Name, request.BrandId))
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Mẫu xe đã tồn tại cho thương hiệu này");
                }

                var model = request.ToEntity();
                await _unitOfWork.VehicleModels.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                var createdModel = await GetModelWithDetailsAsync(model.Id);

                _logger.LogInformation("Created vehicle model {ModelName} (ID: {ModelId}) for brand {BrandName}",
                    model.Name, model.Id, brand!.Name);

                return ApiResponse<ModelResponse>.SuccessResponse(
                    createdModel!.ToResponse(),
                    "Tạo mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle model");
                return ApiResponse<ModelResponse>.FailureResponse("Lỗi khi tạo mẫu xe");
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

        public async Task<ApiResponse<List<ModelResponse>>> GetAllModelsAsync(ModelFilterRequest filterRequest)
        {
            try
            {
                var query = _unitOfWork.VehicleModels.AsQueryable()
                    .Include(m => m.Brand)
                    .Include(m => m.Type)
                    .Where(m => m.DeletedAt == null);

                // Apply filters by ID (more efficient than name search)
                if (filterRequest.TypeId.HasValue)
                {
                    query = query.Where(m => m.TypeId == filterRequest.TypeId.Value);
                }

                if (filterRequest.BrandId.HasValue)
                {
                    query = query.Where(m => m.BrandId == filterRequest.BrandId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filterRequest.ModelName))
                {
                    query = query.Where(m => m.Name.Contains(filterRequest.ModelName));
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
                    query = query.Where(m => m.ReleaseYear == filterRequest.ReleaseYear.Value);
                }

                var totalCount = await query.CountAsync();

                query = filterRequest.IsDescending
                    ? query.OrderByDescending(m => m.CreatedAt)
                    : query.OrderBy(m => m.CreatedAt);

                var items = await query
                    .Skip((filterRequest.PageNumber - 1) * filterRequest.PageSize)
                    .Take(filterRequest.PageSize)
                    .ToListAsync();

                var modelResponses = items.Select(m => m.ToResponse()).ToList();

                return ApiResponse<ModelResponse>.SuccessPagedResponse(
                    modelResponses,
                    totalCount,
                    filterRequest.PageNumber,
                    filterRequest.PageSize,
                    "Lấy danh sách mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicle models");
                return ApiResponse<List<ModelResponse>>.FailureResponse("Lỗi khi lấy danh sách mẫu xe");
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
                    updatedModel!.ToResponse(),
                    "Cập nhật mẫu xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle model with ID: {ModelId}", id);
                return ApiResponse<ModelResponse>.FailureResponse("Lỗi khi cập nhật mẫu xe");
            }
        }

        public async Task<ApiResponse<BulkModelResponse>> BulkCreateModelsAsync(BulkModelRequest request)
        {
            var response = new BulkModelResponse();

            try
            {
                var (isValid, brand, type, errorMessage) = await ValidateTypeBrandRelationshipAsync(request.TypeId, request.BrandId);
                if (!isValid)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse(errorMessage!);
                }

                var existingModelNames = (await _unitOfWork.VehicleModels
                    .GetAllAsync(m => m.BrandId == request.BrandId && m.DeletedAt == null))
                    .Select(m => m.Name.ToLower())
                    .ToHashSet();

                for (int i = 0; i < request.Models.Count; i++)
                {
                    var modelItem = request.Models[i];

                    try
                    {
                        if (existingModelNames.Contains(modelItem.Name.ToLower()))
                        {
                            AddBulkError(response, i, modelItem.Name, "Mẫu xe đã tồn tại cho thương hiệu này");
                            continue;
                        }

                        var model = CreateModelEntity(modelItem, request.BrandId, request.TypeId);
                        await _unitOfWork.VehicleModels.AddAsync(model);

                        existingModelNames.Add(modelItem.Name.ToLower());
                        response.SuccessCount++;

                        _logger.LogInformation("Bulk created model {ModelName} for brand {BrandName}",
                            modelItem.Name, brand!.Name);
                    }
                    catch (Exception ex)
                    {
                        AddBulkError(response, i, modelItem.Name, ex.Message);
                        _logger.LogError(ex, "Error creating model in bulk: {ModelName}", modelItem.Name);
                    }
                }

                if (response.SuccessCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();

                    var createdModels = await _unitOfWork.VehicleModels.AsQueryable()
                        .Include(m => m.Brand)
                        .Include(m => m.Type)
                        .Where(m => m.BrandId == request.BrandId && m.DeletedAt == null)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(response.SuccessCount)
                        .ToListAsync();

                    response.SuccessfulModels = createdModels.Select(m => m.ToResponse()).ToList();
                }

                _logger.LogInformation("Bulk create models completed for {BrandName} ({TypeName}). Success: {SuccessCount}, Failed: {FailedCount}",
                    brand!.Name, type!.Name, response.SuccessCount, response.FailedCount);

                return ApiResponse<BulkModelResponse>.SuccessResponse(
                    response,
                    $"Đã thêm {response.SuccessCount} mẫu xe {brand.Name} ({type.Name}), {response.FailedCount} thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create models");
                return ApiResponse<BulkModelResponse>.FailureResponse("Lỗi khi tạo hàng loạt mẫu xe");
            }
        }

        public async Task<ApiResponse<BulkModelResponse>> BulkCreateModelsFromFileAsync(BulkModelFileRequest request)
        {
            var response = new BulkModelResponse();

            try
            {
                var brand = await _unitOfWork.VehicleBrands
                    .FindOneAsync(b => b.Name == request.BrandName && b.DeletedAt == null);
                if (brand == null)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse($"Không tìm thấy thương hiệu với tên: {request.BrandName}");
                }

                var type = await _unitOfWork.VehicleTypes
                    .FindOneAsync(t => t.Name == request.TypeName && t.DeletedAt == null);
                if (type == null)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse($"Không tìm thấy loại xe với tên: {request.TypeName}");
                }

                var (isValid, _, _, errorMessage) = await ValidateTypeBrandRelationshipAsync(type.Id, brand.Id);
                if (!isValid)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse(errorMessage!);
                }

                var existingModelNames = (await _unitOfWork.VehicleModels
                    .GetAllAsync(m => m.BrandId == brand.Id && m.DeletedAt == null))
                    .Select(m => m.Name.ToLower())
                    .ToHashSet();

                for (int i = 0; i < request.Models.Count; i++)
                {
                    var modelItem = request.Models[i];

                    try
                    {
                        if (existingModelNames.Contains(modelItem.Name.ToLower()))
                        {
                            AddBulkError(response, i, modelItem.Name, "Mẫu xe đã tồn tại cho thương hiệu này");
                            continue;
                        }

                        var model = CreateModelEntity(modelItem, brand.Id, type.Id);
                        await _unitOfWork.VehicleModels.AddAsync(model);

                        existingModelNames.Add(modelItem.Name.ToLower());
                        response.SuccessCount++;

                        _logger.LogInformation("Bulk created model from file {ModelName} for brand {BrandName}",
                            modelItem.Name, brand.Name);
                    }
                    catch (Exception ex)
                    {
                        AddBulkError(response, i, modelItem.Name, ex.Message);
                        _logger.LogError(ex, "Error creating model in bulk from file: {ModelName}", modelItem.Name);
                    }
                }

                if (response.SuccessCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();

                    var createdModels = await _unitOfWork.VehicleModels.AsQueryable()
                        .Include(m => m.Brand)
                        .Include(m => m.Type)
                        .Where(m => m.BrandId == brand.Id && m.DeletedAt == null)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(response.SuccessCount)
                        .ToListAsync();

                    response.SuccessfulModels = createdModels.Select(m => m.ToResponse()).ToList();
                }

                _logger.LogInformation("Bulk create models from file completed for {BrandName} ({TypeName}). Success: {SuccessCount}, Failed: {FailedCount}",
                    brand.Name, type.Name, response.SuccessCount, response.FailedCount);

                return ApiResponse<BulkModelResponse>.SuccessResponse(
                    response,
                    $"Đã thêm {response.SuccessCount} mẫu xe {brand.Name} ({type.Name}), {response.FailedCount} thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create models from file");
                return ApiResponse<BulkModelResponse>.FailureResponse("Lỗi khi tạo hàng loạt mẫu xe từ file");
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

            var typeBrandExists = await _unitOfWork.VehicleTypeBrands.ExistsAsync(typeId, brandId);
            if (!typeBrandExists)
            {
                return (false, null, null,
                    $"Thương hiệu '{brand.Name}' không hỗ trợ loại xe '{type.Name}'. Vui lòng chọn thương hiệu phù hợp với loại xe.");
            }

            return (true, brand, type, null);
        }

        private async Task<bool> ModelNameExistsAsync(string name, Guid brandId, Guid? excludeId = null)
        {
            var existingModel = await _unitOfWork.VehicleModels
                .FindOneAsync(m => m.Name == name
                    && m.BrandId == brandId
                    && m.DeletedAt == null);

            return existingModel != null && (!excludeId.HasValue || existingModel.Id != excludeId.Value);
        }

        private async Task<VehicleModel?> GetModelWithDetailsAsync(Guid id)
        {
            return await _unitOfWork.VehicleModels.AsQueryable()
                .Include(m => m.Brand)
                .Include(m => m.Type)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        private static VehicleModel CreateModelEntity(BulkModelItem item, Guid brandId, Guid typeId)
        {
            return new VehicleModel
            {
                Name = item.Name,
                BrandId = brandId,
                TypeId = typeId,
                ReleaseYear = item.ReleaseYear,
                FuelType = item.FuelType,
                TransmissionType = item.TransmissionType,
                ImageUrl = item.ImageUrl,
                EngineDisplacement = item.EngineDisplacement,
                EngineCapacity = item.EngineCapacity,
                OilCapacity = item.OilCapacity,
                TireSizeFront = item.TireSizeFront,
                TireSizeRear = item.TireSizeRear
            };
        }

        private static void AddBulkError(BulkModelResponse response, int index, string itemName, string errorMessage)
        {
            response.FailedCount++;
            response.Errors.Add(new BulkOperationError
            {
                Index = index,
                ItemName = itemName,
                ErrorMessage = errorMessage
            });
        }
    }
}
