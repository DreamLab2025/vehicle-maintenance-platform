using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartProductService(ILogger<PartProductService> logger, IUnitOfWork unitOfWork) : IPartProductService
    {
        private readonly ILogger<PartProductService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<PartProductResponse>> CreateProductAsync(PartProductRequest request)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(request.PartCategoryId);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
                }

                var product = request.ToEntity();
                await _unitOfWork.PartProducts.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                var createdProduct = await _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                _logger.LogInformation("Created part product {ProductName} (ID: {ProductId})", product.Name, product.Id);

                return ApiResponse<PartProductResponse>.CreatedResponse(
                    createdProduct!.ToResponse(),
                    "Tạo phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part product {ProductName}", request.Name);
                return ApiResponse<PartProductResponse>.FailureResponse("Lỗi khi tạo phụ tùng");
            }
        }

        public async Task<ApiResponse<PartProductResponse>> UpdateProductAsync(Guid id, PartProductRequest request)
        {
            try
            {
                var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
                if (product == null || product.DeletedAt != null)
                {
                    return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy phụ tùng");
                }

                var category = await _unitOfWork.PartCategories.GetByIdAsync(request.PartCategoryId);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
                }

                product.UpdateEntity(request);
                await _unitOfWork.PartProducts.UpdateAsync(id, product);
                await _unitOfWork.SaveChangesAsync();

                var updatedProduct = await _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                _logger.LogInformation("Updated part product {ProductName} (ID: {ProductId})", product.Name, id);

                return ApiResponse<PartProductResponse>.SuccessResponse(
                    updatedProduct!.ToResponse(),
                    "Cập nhật phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part product with ID: {ProductId}", id);
                return ApiResponse<PartProductResponse>.FailureResponse("Lỗi khi cập nhật phụ tùng");
            }
        }

        public async Task<ApiResponse<string>> DeleteProductAsync(Guid id)
        {
            try
            {
                var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
                if (product == null || product.DeletedAt != null)
                {
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy phụ tùng");
                }

                await _unitOfWork.PartProducts.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted part product {ProductName} (ID: {ProductId})", product.Name, id);

                return ApiResponse<string>.SuccessResponse("Đã xóa", "Xóa phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part product with ID: {ProductId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa phụ tùng");
            }
        }

        public async Task<ApiResponse<List<PartProductResponse>>> GetProductsByCategoryAsync(Guid categoryId, PaginationRequest paginationRequest)
        {
            try
            {
                paginationRequest.Normalize();
                var category = await _unitOfWork.PartCategories.GetByIdAsync(categoryId);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<List<PartProductResponse>>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
                }

                var query = _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .Where(p => p.PartCategoryId == categoryId && p.DeletedAt == null)
                    .OrderBy(p => p.Name);

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .ToListAsync();

                var responses = products.Select(p => p.ToResponse()).ToList();

                return ApiResponse<List<PartProductResponse>>.SuccessPagedResponse(
                    responses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category ID: {CategoryId}", categoryId);
                return ApiResponse<List<PartProductResponse>>.FailureResponse("Lỗi khi lấy danh sách phụ tùng");
            }
        }

        public async Task<ApiResponse<PartProductResponse>> GetProductByIdAsync(Guid id)
        {
            try
            {
                var product = await _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

                if (product == null)
                {
                    return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy phụ tùng");
                }

                return ApiResponse<PartProductResponse>.SuccessResponse(
                    product.ToResponse(),
                    "Lấy thông tin phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part product with ID: {ProductId}", id);
                return ApiResponse<PartProductResponse>.FailureResponse("Lỗi khi lấy thông tin phụ tùng");
            }
        }
    }
}
