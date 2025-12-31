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
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(request.BrandId);
                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Thương hiệu không tồn tại");
                }

                var type = await _unitOfWork.VehicleTypes.GetByIdAsync(request.TypeId);
                if (type == null || type.DeletedAt != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Loại xe không tồn tại");
                }

                var existingModel = await _unitOfWork.VehicleModels
                    .FindOneAsync(m => m.Name == request.Name
                                    && m.BrandId == request.BrandId
                                    && m.DeletedAt == null);

                if (existingModel != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Mẫu xe đã tồn tại cho thương hiệu này");
                }

                var model = request.ToEntity();
                await _unitOfWork.VehicleModels.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                var createdModel = await _unitOfWork.VehicleModels.AsQueryable()
                    .Include(m => m.Brand)
                    .Include(m => m.Type)
                    .FirstOrDefaultAsync(m => m.Id == model.Id);

                _logger.LogInformation("Created vehicle model with ID: {ModelId}", model.Id);

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

        public async Task<ApiResponse<List<ModelResponse>>> GetAllModelsAsync(PaginationRequest paginationRequest)
        {
            try
            {
                var query = _unitOfWork.VehicleModels.AsQueryable()
                    .Include(m => m.Brand)
                    .Include(m => m.Type)
                    .Where(m => m.DeletedAt == null);

                var totalCount = await query.CountAsync();

                if (paginationRequest.IsDescending)
                {
                    query = query.OrderByDescending(m => m.CreatedAt);
                }
                else
                {
                    query = query.OrderBy(m => m.CreatedAt);
                }

                var items = await query
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .ToListAsync();

                var modelResponses = items.Select(m => m.ToResponse()).ToList();

                return ApiResponse<ModelResponse>.SuccessPagedResponse(
                    modelResponses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
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

                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(request.BrandId);
                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Thương hiệu không tồn tại");
                }

                var type = await _unitOfWork.VehicleTypes.GetByIdAsync(request.TypeId);
                if (type == null || type.DeletedAt != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Loại xe không tồn tại");
                }

                var existingModel = await _unitOfWork.VehicleModels
                    .FindOneAsync(m => m.Name == request.Name
                                    && m.BrandId == request.BrandId
                                    && m.Id != id
                                    && m.DeletedAt == null);

                if (existingModel != null)
                {
                    return ApiResponse<ModelResponse>.FailureResponse("Tên mẫu xe đã tồn tại cho thương hiệu này");
                }

                model.UpdateEntity(request);
                await _unitOfWork.VehicleModels.UpdateAsync(id, model);
                await _unitOfWork.SaveChangesAsync();

                var updatedModel = await _unitOfWork.VehicleModels.AsQueryable()
                    .Include(m => m.Brand)
                    .Include(m => m.Type)
                    .FirstOrDefaultAsync(m => m.Id == id);

                _logger.LogInformation("Updated vehicle model with ID: {ModelId}", id);

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
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(request.BrandId);
                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse("Thương hiệu không tồn tại");
                }

                var type = await _unitOfWork.VehicleTypes.GetByIdAsync(request.TypeId);
                if (type == null || type.DeletedAt != null)
                {
                    return ApiResponse<BulkModelResponse>.FailureResponse("Loại xe không tồn tại");
                }

                var existingModels = await _unitOfWork.VehicleModels
                    .GetAllAsync(m => m.BrandId == request.BrandId && m.DeletedAt == null);
                var existingModelNames = existingModels
                    .Select(m => m.Name.ToLower())
                    .ToHashSet();

                for (int i = 0; i < request.Models.Count; i++)
                {
                    var modelItem = request.Models[i];

                    try
                    {
                        if (existingModelNames.Contains(modelItem.Name.ToLower()))
                        {
                            response.FailedCount++;
                            response.Errors.Add(new BulkOperationError
                            {
                                Index = i,
                                ItemName = modelItem.Name,
                                ErrorMessage = "Mẫu xe đã tồn tại cho thương hiệu này"
                            });
                            continue;
                        }

                        var model = new VehicleModel
                        {
                            Name = modelItem.Name,
                            BrandId = request.BrandId,
                            TypeId = request.TypeId,
                            ReleaseYear = modelItem.ReleaseYear,
                            FuelType = modelItem.FuelType,
                            ImageUrl = modelItem.ImageUrl,
                            OilCapacity = modelItem.OilCapacity,
                            TireSizeFront = modelItem.TireSizeFront,
                            TireSizeRear = modelItem.TireSizeRear
                        };

                        await _unitOfWork.VehicleModels.AddAsync(model);

                        existingModelNames.Add(modelItem.Name.ToLower());

                        response.SuccessCount++;

                        _logger.LogInformation("Bulk created model: {ModelName} for brand: {BrandName}",
                            modelItem.Name, brand.Name);
                    }
                    catch (Exception ex)
                    {
                        response.FailedCount++;
                        response.Errors.Add(new BulkOperationError
                        {
                            Index = i,
                            ItemName = modelItem.Name,
                            ErrorMessage = ex.Message
                        });
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
                    brand.Name, type.Name, response.SuccessCount, response.FailedCount);

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
    }
}
