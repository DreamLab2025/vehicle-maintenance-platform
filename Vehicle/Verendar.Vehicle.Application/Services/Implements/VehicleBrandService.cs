using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class VehicleBrandService : IVehicleBrandService
    {
        private readonly ILogger<VehicleBrandService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleBrandService(ILogger<VehicleBrandService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<BrandResponse>> CreateBrandAsync(BrandRequest request)
        {
            try
            {
                if (await BrandNameExistsAsync(request.Name))
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Thương hiệu đã tồn tại");
                }

                var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Không tìm thấy loại xe");
                }

                var brand = request.ToEntity();
                brand.VehicleTypeId = request.VehicleTypeId;
                await _unitOfWork.VehicleBrands.AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created brand {BrandName} (ID: {BrandId}) for type: {TypeName}",
                    brand.Name, brand.Id, vehicleType.Name);

                var createdBrand = await _unitOfWork.VehicleBrands.GetByIdWithTypesAsync(brand.Id);
                return ApiResponse<BrandResponse>.SuccessResponse(
                    createdBrand!.ToResponse(),
                    "Tạo thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand {BrandName}", request.Name);
                return ApiResponse<BrandResponse>.FailureResponse("Lỗi khi tạo thương hiệu");
            }
        }

        public async Task<ApiResponse<string>> DeleteBrandAsync(Guid id)
        {
            try
            {
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(id);
                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy thương hiệu");
                }

                await _unitOfWork.VehicleBrands.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted brand {BrandName} (ID: {BrandId})", brand.Name, id);

                return ApiResponse<string>.SuccessResponse("Deleted", "Xóa thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand with ID: {BrandId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa thương hiệu");
            }
        }

        public async Task<ApiResponse<List<BrandResponse>>> GetAllBrandsAsync(PaginationRequest paginationRequest)
        {
            try
            {
                var query = _unitOfWork.VehicleBrands.AsQueryable()
                    .Where(b => b.DeletedAt == null);

                var totalCount = await query.CountAsync();

                if (paginationRequest.IsDescending.HasValue)
                {
                    query = paginationRequest.IsDescending.Value
                        ? query.OrderByDescending(b => b.CreatedAt)
                        : query.OrderBy(b => b.CreatedAt);
                }

                var items = await query
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .ToListAsync();

                var brandResponses = new List<BrandResponse>();
                foreach (var brand in items)
                {
                    var brandWithTypes = await _unitOfWork.VehicleBrands.GetByIdWithTypesAsync(brand.Id);
                    brandResponses.Add(brandWithTypes!.ToResponse());
                }

                return ApiResponse<List<BrandResponse>>.SuccessPagedResponse(
                    brandResponses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return ApiResponse<List<BrandResponse>>.FailureResponse("Lỗi khi lấy danh sách thương hiệu");
            }
        }

        public async Task<ApiResponse<List<BrandResponse>>> GetBrandsByTypeIdAsync(Guid typeId)
        {
            try
            {
                var type = await _unitOfWork.VehicleTypes.GetByIdAsync(typeId);
                if (type == null || type.DeletedAt != null)
                {
                    return ApiResponse<List<BrandResponse>>.FailureResponse("Không tìm thấy loại xe");
                }

                var brands = await _unitOfWork.VehicleBrands
                    .GetAllAsync(b => b.VehicleTypeId == typeId && b.DeletedAt == null);

                var brandResponses = new List<BrandResponse>();
                foreach (var brand in brands)
                {
                    var brandWithType = await _unitOfWork.VehicleBrands.GetByIdWithTypesAsync(brand.Id);
                    if (brandWithType != null)
                    {
                        brandResponses.Add(brandWithType.ToResponse());
                    }
                }

                return ApiResponse<List<BrandResponse>>.SuccessResponse(
                    brandResponses,
                    $"Lấy danh sách {brandResponses.Count} thương hiệu của loại xe '{type.Name}' thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands by type ID: {TypeId}", typeId);
                return ApiResponse<List<BrandResponse>>.FailureResponse("Lỗi khi lấy danh sách thương hiệu theo loại xe");
            }
        }

        public async Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request)
        {
            try
            {
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(id);
                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Không tìm thấy thương hiệu");
                }

                if (await BrandNameExistsAsync(request.Name, id))
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Tên thương hiệu đã tồn tại");
                }

                var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Không tìm thấy loại xe");
                }

                brand.UpdateEntity(request);
                brand.VehicleTypeId = request.VehicleTypeId;
                await _unitOfWork.VehicleBrands.UpdateAsync(id, brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated brand {BrandName} (ID: {BrandId}) with type: {TypeName}",
                    brand.Name, id, vehicleType.Name);

                var updatedBrand = await _unitOfWork.VehicleBrands.GetByIdWithTypesAsync(id);
                return ApiResponse<BrandResponse>.SuccessResponse(
                    updatedBrand!.ToResponse(),
                    "Cập nhật thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand with ID: {BrandId}", id);
                return ApiResponse<BrandResponse>.FailureResponse("Lỗi khi cập nhật thương hiệu");
            }
        }

        private async Task<bool> BrandNameExistsAsync(string name, Guid? excludeId = null)
        {
            var existingBrand = await _unitOfWork.VehicleBrands
                .FindOneAsync(b => b.Name == name && b.DeletedAt == null);

            return existingBrand != null && (!excludeId.HasValue || existingBrand.Id != excludeId.Value);
        }
    }
}
