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
            var category = await _unitOfWork.PartCategories.GetByIdAsync(request.PartCategoryId);
            if (category == null || category.DeletedAt != null)
            {
                _logger.LogWarning("CreateProduct: category not found {PartCategoryId}", request.PartCategoryId);
                return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            var product = request.ToEntity();
            await _unitOfWork.PartProducts.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _unitOfWork.PartProducts.GetByIdWithCategoryAsync(product.Id);

            return ApiResponse<PartProductResponse>.CreatedResponse(
                createdProduct!.ToResponse(),
                "Tạo phụ tùng thành công");
        }

        public async Task<ApiResponse<PartProductResponse>> UpdateProductAsync(Guid id, PartProductRequest request)
        {
            var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
            if (product == null || product.DeletedAt != null)
            {
                _logger.LogWarning("UpdateProduct: not found {ProductId}", id);
                return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy phụ tùng");
            }

            var category = await _unitOfWork.PartCategories.GetByIdAsync(request.PartCategoryId);
            if (category == null || category.DeletedAt != null)
            {
                _logger.LogWarning("UpdateProduct: category not found {PartCategoryId}", request.PartCategoryId);
                return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            product.UpdateEntity(request);
            await _unitOfWork.PartProducts.UpdateAsync(id, product);
            await _unitOfWork.SaveChangesAsync();

            var updatedProduct = await _unitOfWork.PartProducts.GetByIdWithCategoryAsync(id);

            return ApiResponse<PartProductResponse>.SuccessResponse(
                updatedProduct!.ToResponse(),
                "Cập nhật phụ tùng thành công");
        }

        public async Task<ApiResponse<string>> DeleteProductAsync(Guid id)
        {
            var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
            if (product == null || product.DeletedAt != null)
            {
                _logger.LogWarning("DeleteProduct: not found {ProductId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy phụ tùng");
            }

            await _unitOfWork.PartProducts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Đã xóa", "Xóa phụ tùng thành công");
        }

        public async Task<ApiResponse<List<PartProductSummary>>> GetProductsByCategoryAsync(Guid categoryId, PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            var category = await _unitOfWork.PartCategories.GetByIdAsync(categoryId);
            if (category == null || category.DeletedAt != null)
            {
                _logger.LogWarning("GetProductsByCategory: category not found {CategoryId}", categoryId);
                return ApiResponse<List<PartProductSummary>>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            var query = _unitOfWork.PartProducts.AsQueryableWithCategory()
                .Where(p => p.PartCategoryId == categoryId && p.DeletedAt == null)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var summaries = products.Select(p => p.ToSummary()).ToList();

            return ApiResponse<List<PartProductSummary>>.SuccessPagedResponse(
                summaries,
                totalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy danh sách phụ tùng thành công");
        }

        public async Task<ApiResponse<PartProductResponse>> GetProductByIdAsync(Guid id)
        {
            var product = await _unitOfWork.PartProducts.GetByIdWithCategoryAsync(id);

            if (product == null)
            {
                _logger.LogWarning("GetProductById: not found {ProductId}", id);
                return ApiResponse<PartProductResponse>.NotFoundResponse("Không tìm thấy phụ tùng");
            }

            return ApiResponse<PartProductResponse>.SuccessResponse(
                product.ToResponse(),
                "Lấy thông tin phụ tùng thành công");
        }
    }
}
