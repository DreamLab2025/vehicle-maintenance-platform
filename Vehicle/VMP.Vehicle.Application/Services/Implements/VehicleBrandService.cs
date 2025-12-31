using Microsoft.Extensions.Logging;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Mappings;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Repositories.Interfaces;

namespace VMP.Vehicle.Application.Services.Implements
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
                var existingBrand = await _unitOfWork.VehicleBrands
                    .FindOneAsync(b => b.Name == request.Name && b.DeletedAt == null);

                if (existingBrand != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Thương hiệu đã tồn tại");
                }

                var brand = request.ToEntity();
                await _unitOfWork.VehicleBrands.AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created brand with ID: {BrandId}", brand.Id);

                return ApiResponse<BrandResponse>.SuccessResponse(
                    brand.ToResponse(),
                    "Tạo thương hiệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
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

                _logger.LogInformation("Deleted brand with ID: {BrandId}", id);

                return ApiResponse<string>.SuccessResponse(
                    "Deleted",
                    "Xóa thương hiệu thành công");
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
                var (items, totalCount) = await _unitOfWork.VehicleBrands.GetPagedAsync(
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    filter: b => b.DeletedAt == null,
                    orderBy: q => paginationRequest.IsDescending
                        ? q.OrderByDescending(b => b.CreatedAt)
                        : q.OrderBy(b => b.CreatedAt)
                );

                var brandResponses = items.Select(b => b.ToResponse()).ToList();

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

        public async Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request)
        {
            try
            {
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(id);

                if (brand == null || brand.DeletedAt != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Không tìm thấy thương hiệu");
                }

                var existingBrand = await _unitOfWork.VehicleBrands
                    .FindOneAsync(b => b.Name == request.Name && b.Id != id && b.DeletedAt == null);

                if (existingBrand != null)
                {
                    return ApiResponse<BrandResponse>.FailureResponse("Tên thương hiệu đã tồn tại");
                }

                brand.UpdateEntity(request);
                await _unitOfWork.VehicleBrands.UpdateAsync(id, brand);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated brand with ID: {BrandId}", id);

                return ApiResponse<BrandResponse>.SuccessResponse(
                    brand.ToResponse(),
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
                var existingBrands = await _unitOfWork.VehicleBrands
                    .GetAllAsync(b => b.DeletedAt == null);
                var existingNames = existingBrands.Select(b => b.Name.ToLower()).ToHashSet();

                for (int i = 0; i < request.Brands.Count; i++)
                {
                    var brandRequest = request.Brands[i];

                    try
                    {
                        if (existingNames.Contains(brandRequest.Name.ToLower()))
                        {
                            response.FailedCount++;
                            response.Errors.Add(new BulkOperationError
                            {
                                Index = i,
                                ItemName = brandRequest.Name,
                                ErrorMessage = "Thương hiệu đã tồn tại"
                            });
                            continue;
                        }

                        var brand = brandRequest.ToEntity();
                        await _unitOfWork.VehicleBrands.AddAsync(brand);

                        existingNames.Add(brandRequest.Name.ToLower());

                        response.SuccessfulBrands.Add(brand.ToResponse());
                        response.SuccessCount++;

                        _logger.LogInformation("Bulk created brand: {BrandName}", brand.Name);
                    }
                    catch (Exception ex)
                    {
                        response.FailedCount++;
                        response.Errors.Add(new BulkOperationError
                        {
                            Index = i,
                            ItemName = brandRequest.Name,
                            ErrorMessage = ex.Message
                        });
                        _logger.LogError(ex, "Error creating brand in bulk: {BrandName}", brandRequest.Name);
                    }
                }

                if (response.SuccessCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
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
    }
}
