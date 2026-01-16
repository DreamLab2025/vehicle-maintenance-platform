using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;
using Verender.Vehicle.Application.Mappings;
using Verender.Vehicle.Application.Services.Interfaces;
using Verender.Vehicle.Domain.Entities;
using Verender.Vehicle.Domain.Repositories.Interfaces;

namespace Verender.Vehicle.Application.Services.Implements
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

                var (isValid, typeNames, errorMessage) = await ValidateVehicleTypesAsync(request.VehicleTypeIds);
                if (!isValid)
                {
                    return ApiResponse<BrandResponse>.FailureResponse(errorMessage!);
                }

                var brand = request.ToEntity();
                await _unitOfWork.VehicleBrands.AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                await CreateTypeBrandRelationshipsAsync(brand.Id, request.VehicleTypeIds);

                _logger.LogInformation("Created brand {BrandName} (ID: {BrandId}) for types: {TypeNames}",
                    brand.Name, brand.Id, string.Join(", ", typeNames!));

                return ApiResponse<BrandResponse>.SuccessResponse(
                    brand.ToResponse(typeNames),
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

                await DeleteTypeBrandRelationshipsAsync(id);
                await _unitOfWork.VehicleBrands.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted brand {BrandName} (ID: {BrandId}) and its relationships", brand.Name, id);

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

                var brands = await _unitOfWork.VehicleTypeBrands.GetBrandsByTypeIdAsync(typeId);
                var brandResponses = brands.Select(b => b.ToResponse()).ToList();

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

                var (isValid, typeNames, errorMessage) = await ValidateVehicleTypesAsync(request.VehicleTypeIds);
                if (!isValid)
                {
                    return ApiResponse<BrandResponse>.FailureResponse(errorMessage!);
                }

                brand.UpdateEntity(request);
                await _unitOfWork.VehicleBrands.UpdateAsync(id, brand);

                await ReplaceTypeBrandRelationshipsAsync(id, request.VehicleTypeIds);

                _logger.LogInformation("Updated brand {BrandName} (ID: {BrandId}) with types: {TypeNames}",
                    brand.Name, id, string.Join(", ", typeNames!));

                return ApiResponse<BrandResponse>.SuccessResponse(
                    brand.ToResponse(typeNames),
                    "Cập nhật thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand with ID: {BrandId}", id);
                return ApiResponse<BrandResponse>.FailureResponse("Lỗi khi cập nhật thương hiệu");
            }
        }

        public async Task<ApiResponse<BulkBrandResponse>> BulkCreateBrandsAsync(BulkBrandRequest request)
        {
            var response = new BulkBrandResponse();

            try
            {
                var existingNames = (await _unitOfWork.VehicleBrands.GetAllAsync(b => b.DeletedAt == null))
                    .Select(b => b.Name.ToLower())
                    .ToHashSet();

                for (int i = 0; i < request.Brands.Count; i++)
                {
                    var brandRequest = request.Brands[i];

                    try
                    {
                        if (existingNames.Contains(brandRequest.Name.ToLower()))
                        {
                            AddBulkError(response, i, brandRequest.Name, "Thương hiệu đã tồn tại");
                            continue;
                        }

                        var (isValid, typeNames, errorMessage) = await ValidateVehicleTypesAsync(brandRequest.VehicleTypeIds);
                        if (!isValid)
                        {
                            AddBulkError(response, i, brandRequest.Name, errorMessage!);
                            continue;
                        }

                        var brand = brandRequest.ToEntity();
                        await _unitOfWork.VehicleBrands.AddAsync(brand);
                        await _unitOfWork.SaveChangesAsync();

                        await CreateTypeBrandRelationshipsAsync(brand.Id, brandRequest.VehicleTypeIds);

                        existingNames.Add(brandRequest.Name.ToLower());
                        response.SuccessfulBrands.Add(brand.ToResponse(typeNames));
                        response.SuccessCount++;

                        _logger.LogInformation("Bulk created brand {BrandName} for types: {TypeNames}",
                            brand.Name, string.Join(", ", typeNames!));
                    }
                    catch (Exception ex)
                    {
                        AddBulkError(response, i, brandRequest.Name, ex.Message);
                        _logger.LogError(ex, "Error creating brand in bulk: {BrandName}", brandRequest.Name);
                    }
                }

                _logger.LogInformation("Bulk create brands completed. Success: {SuccessCount}, Failed: {FailedCount}",
                    response.SuccessCount, response.FailedCount);

                return ApiResponse<BulkBrandResponse>.SuccessResponse(
                    response,
                    $"Đã thêm {response.SuccessCount} thương hiệu, {response.FailedCount} thất bại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create brands");
                return ApiResponse<BulkBrandResponse>.FailureResponse("Lỗi khi tạo hàng loạt thương hiệu");
            }
        }

        private async Task<bool> BrandNameExistsAsync(string name, Guid? excludeId = null)
        {
            var existingBrand = await _unitOfWork.VehicleBrands
                .FindOneAsync(b => b.Name == name && b.DeletedAt == null);

            return existingBrand != null && (!excludeId.HasValue || existingBrand.Id != excludeId.Value);
        }

        private async Task<(bool IsValid, List<string>? TypeNames, string? ErrorMessage)> ValidateVehicleTypesAsync(List<Guid> typeIds)
        {
            var typeNames = new List<string>();

            foreach (var typeId in typeIds)
            {
                var type = await _unitOfWork.VehicleTypes.GetByIdAsync(typeId);
                if (type == null || type.DeletedAt != null)
                {
                    return (false, null, $"Không tìm thấy loại xe với ID: {typeId}");
                }
                typeNames.Add(type.Name);
            }

            return (true, typeNames, null);
        }

        private async Task CreateTypeBrandRelationshipsAsync(Guid brandId, List<Guid> typeIds)
        {
            foreach (var typeId in typeIds)
            {
                var typeBrand = new VehicleTypeBrand
                {
                    VehicleTypeId = typeId,
                    VehicleBrandId = brandId
                };
                await _unitOfWork.VehicleTypeBrands.AddAsync(typeBrand);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task DeleteTypeBrandRelationshipsAsync(Guid brandId)
        {
            var typeBrands = await _unitOfWork.VehicleTypeBrands
                .GetAllAsync(vtb => vtb.VehicleBrandId == brandId && vtb.DeletedAt == null);

            foreach (var typeBrand in typeBrands)
            {
                await _unitOfWork.VehicleTypeBrands.DeleteAsync(typeBrand.Id);
            }
        }

        private async Task ReplaceTypeBrandRelationshipsAsync(Guid brandId, List<Guid> newTypeIds)
        {
            await DeleteTypeBrandRelationshipsAsync(brandId);
            await CreateTypeBrandRelationshipsAsync(brandId, newTypeIds);
        }

        private static void AddBulkError(BulkBrandResponse response, int index, string itemName, string errorMessage)
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
