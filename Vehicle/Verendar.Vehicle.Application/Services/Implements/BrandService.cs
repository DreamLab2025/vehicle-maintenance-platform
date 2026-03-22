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
            if (await BrandNameExistsAsync(request.Name))
            {
                _logger.LogWarning("CreateBrand: duplicate name {BrandName}", request.Name);
                return ApiResponse<BrandResponse>.ConflictResponse("Thương hiệu đã tồn tại");
            }

            var vehicleType = await _unitOfWork.Types.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null)
            {
                _logger.LogWarning("CreateBrand: vehicle type not found {VehicleTypeId}", request.VehicleTypeId);
                return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy loại xe");
            }

            var brand = request.ToEntity();
            brand.VehicleTypeId = request.VehicleTypeId;
            await _unitOfWork.Brands.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            var createdBrand = await _unitOfWork.Brands.GetByIdWithTypesAsync(brand.Id);
            return ApiResponse<BrandResponse>.CreatedResponse(
                createdBrand!.ToResponse(),
                "Tạo thương hiệu thành công");
        }

        public async Task<ApiResponse<string>> DeleteBrandAsync(Guid id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
            {
                _logger.LogWarning("DeleteBrand: not found {BrandId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy thương hiệu");
            }

            await _unitOfWork.Brands.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Deleted", "Xóa thương hiệu thành công");
        }

        public async Task<ApiResponse<List<BrandSummary>>> GetAllBrandsAsync(PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            IQueryable<Brand> query = _unitOfWork.Brands.AsQueryableWithVehicleType();

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

        public async Task<ApiResponse<List<BrandSummary>>> GetBrandsByTypeIdAsync(Guid typeId)
        {
            var type = await _unitOfWork.Types.GetByIdAsync(typeId);
            if (type == null)
            {
                _logger.LogWarning("GetBrandsByTypeId: type not found {TypeId}", typeId);
                return ApiResponse<List<BrandSummary>>.NotFoundResponse("Không tìm thấy loại xe");
            }

            var brands = await _unitOfWork.Brands.AsQueryableWithVehicleType()
                .Where(b => b.VehicleTypeId == typeId)
                .ToListAsync();

            return ApiResponse<List<BrandSummary>>.SuccessResponse(
                brands.Select(b => b.ToSummary()).ToList(),
                $"Lấy danh sách {brands.Count} thương hiệu của loại xe '{type.Name}' thành công");
        }

        public async Task<ApiResponse<BrandResponse>> GetBrandByIdAsync(Guid id)
        {
            var brand = await _unitOfWork.Brands.GetByIdWithTypesAsync(id);
            if (brand == null)
            {
                _logger.LogWarning("GetBrandById: not found {BrandId}", id);
                return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy thương hiệu");
            }

            return ApiResponse<BrandResponse>.SuccessResponse(
                brand.ToResponse(),
                "Lấy thông tin thương hiệu thành công");
        }

        public async Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
            {
                _logger.LogWarning("UpdateBrand: not found {BrandId}", id);
                return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy thương hiệu");
            }

            if (await BrandNameExistsAsync(request.Name, id))
            {
                _logger.LogWarning("UpdateBrand: name conflict {BrandName} exclude {BrandId}", request.Name, id);
                return ApiResponse<BrandResponse>.ConflictResponse("Tên thương hiệu đã tồn tại");
            }

            var vehicleType = await _unitOfWork.Types.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null)
            {
                _logger.LogWarning("UpdateBrand: vehicle type not found {VehicleTypeId}", request.VehicleTypeId);
                return ApiResponse<BrandResponse>.NotFoundResponse("Không tìm thấy loại xe");
            }

            brand.UpdateEntity(request);
            brand.VehicleTypeId = request.VehicleTypeId;
            await _unitOfWork.Brands.UpdateAsync(id, brand);
            await _unitOfWork.SaveChangesAsync();

            var updatedBrand = await _unitOfWork.Brands.GetByIdWithTypesAsync(id);
            return ApiResponse<BrandResponse>.SuccessResponse(
                updatedBrand!.ToResponse(),
                "Cập nhật thương hiệu thành công");
        }

        private async Task<bool> BrandNameExistsAsync(string name, Guid? excludeId = null)
        {
            var existingBrand = await _unitOfWork.Brands
                .FindOneAsync(b => b.Name == name && b.DeletedAt == null);

            return existingBrand != null && (!excludeId.HasValue || existingBrand.Id != excludeId.Value);
        }
    }
}
