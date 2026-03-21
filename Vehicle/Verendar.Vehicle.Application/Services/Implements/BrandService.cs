using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class BrandService(ILogger<BrandService> logger, IUnitOfWork unitOfWork) : IBrandService
    {
        private readonly ILogger<BrandService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<BrandResponse>> CreateBrandAsync(BrandRequest request)
        {
            try
            {
                if (await BrandNameExistsAsync(request.Name))
                {
                    return ApiResponse<BrandResponse>.ConflictResponse("Thương hiệu đã tồn tại");
                }

                var vehicleType = await _unitOfWork.Types.GetByIdAsync(request.VehicleTypeId);
                if (vehicleType == null)
                {
                    return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy loại xe");
                }

                var brand = request.ToEntity();
                brand.VehicleTypeId = request.VehicleTypeId;
                await _unitOfWork.Brands.AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created brand {BrandName} (ID: {BrandId}) for type: {TypeName}",
                    brand.Name, brand.Id, vehicleType.Name);

                var createdBrand = await _unitOfWork.Brands.GetByIdWithTypesAsync(brand.Id);
                return ApiResponse<BrandResponse>.CreatedResponse(
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
                var brand = await _unitOfWork.Brands.GetByIdAsync(id);
                if (brand == null)
                {
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy thương hiệu");
                }

                await _unitOfWork.Brands.DeleteAsync(id);
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

        public async Task<ApiResponse<List<BrandSummary>>> GetAllBrandsAsync(PaginationRequest paginationRequest)
        {
            try
            {
                paginationRequest.Normalize();
                IQueryable<Brand> query = _unitOfWork.Brands.AsQueryable()
                    .Include(b => b.VehicleType);

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

                return ApiResponse<List<BrandSummary>>.SuccessPagedResponse(
                    items.Select(b => b.ToSummary()).ToList(),
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return ApiResponse<List<BrandSummary>>.FailureResponse("Lỗi khi lấy danh sách thương hiệu");
            }
        }

        public async Task<ApiResponse<List<BrandSummary>>> GetBrandsByTypeIdAsync(Guid typeId)
        {
            try
            {
                var type = await _unitOfWork.Types.GetByIdAsync(typeId);
                if (type == null)
                {
                    return ApiResponse<List<BrandSummary>>.NotFoundResponse("Không tìm thấy loại xe");
                }

                var brands = await _unitOfWork.Brands.AsQueryable()
                    .Include(b => b.VehicleType)
                    .Where(b => b.VehicleTypeId == typeId)
                    .ToListAsync();

                return ApiResponse<List<BrandSummary>>.SuccessResponse(
                    brands.Select(b => b.ToSummary()).ToList(),
                    $"Lấy danh sách {brands.Count} thương hiệu của loại xe '{type.Name}' thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands by type ID: {TypeId}", typeId);
                return ApiResponse<List<BrandSummary>>.FailureResponse("Lỗi khi lấy danh sách thương hiệu theo loại xe");
            }
        }

        public async Task<ApiResponse<BrandResponse>> GetBrandByIdAsync(Guid id)
        {
            try
            {
                var brand = await _unitOfWork.Brands.GetByIdWithTypesAsync(id);
                if (brand == null)
                {
                    return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy thương hiệu");
                }

                return ApiResponse<BrandResponse>.SuccessResponse(
                    brand.ToResponse(),
                    "Lấy thông tin thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand with ID: {BrandId}", id);
                return ApiResponse<BrandResponse>.FailureResponse("Lỗi khi lấy thông tin thương hiệu");
            }
        }

        public async Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request)
        {
            try
            {
                var brand = await _unitOfWork.Brands.GetByIdAsync(id);
                if (brand == null)
                {
                    return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy thương hiệu");
                }

                if (await BrandNameExistsAsync(request.Name, id))
                {
                    return ApiResponse<BrandResponse>.ConflictResponse("Tên thương hiệu đã tồn tại");
                }

                var vehicleType = await _unitOfWork.Types.GetByIdAsync(request.VehicleTypeId);
                if (vehicleType == null)
                {
                    return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy loại xe");
                }

                brand.UpdateEntity(request);
                brand.VehicleTypeId = request.VehicleTypeId;
                await _unitOfWork.Brands.UpdateAsync(id, brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated brand {BrandName} (ID: {BrandId}) with type: {TypeName}",
                    brand.Name, id, vehicleType.Name);

                var updatedBrand = await _unitOfWork.Brands.GetByIdWithTypesAsync(id);
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
            var existingBrand = await _unitOfWork.Brands
                .FindOneAsync(b => b.Name == name && b.DeletedAt == null);

            return existingBrand != null && (!excludeId.HasValue || existingBrand.Id != excludeId.Value);
        }
    }
}
