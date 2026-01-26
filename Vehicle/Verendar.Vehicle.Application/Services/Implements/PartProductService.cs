using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

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
                    return ApiResponse<PartProductResponse>.FailureResponse("Category not found");
                }

                var product = request.ToEntity();
                await _unitOfWork.PartProducts.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                var createdProduct = await _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                _logger.LogInformation("Created part product {ProductName} (ID: {ProductId})", product.Name, product.Id);

                return ApiResponse<PartProductResponse>.SuccessResponse(
                    createdProduct!.ToResponse(),
                    "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part product {ProductName}", request.Name);
                return ApiResponse<PartProductResponse>.FailureResponse("Error creating product");
            }
        }

        public async Task<ApiResponse<PartProductResponse>> UpdateProductAsync(Guid id, PartProductRequest request)
        {
            try
            {
                var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
                if (product == null || product.DeletedAt != null)
                {
                    return ApiResponse<PartProductResponse>.FailureResponse("Product not found");
                }

                var category = await _unitOfWork.PartCategories.GetByIdAsync(request.PartCategoryId);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartProductResponse>.FailureResponse("Category not found");
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
                    "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part product with ID: {ProductId}", id);
                return ApiResponse<PartProductResponse>.FailureResponse("Error updating product");
            }
        }

        public async Task<ApiResponse<string>> DeleteProductAsync(Guid id)
        {
            try
            {
                var product = await _unitOfWork.PartProducts.GetByIdAsync(id);
                if (product == null || product.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Product not found");
                }

                await _unitOfWork.PartProducts.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted part product {ProductName} (ID: {ProductId})", product.Name, id);

                return ApiResponse<string>.SuccessResponse("Deleted", "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part product with ID: {ProductId}", id);
                return ApiResponse<string>.FailureResponse("Error deleting product");
            }
        }

        public async Task<ApiResponse<List<PartProductResponse>>> GetProductsByCategoryAsync(Guid categoryId)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(categoryId);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<List<PartProductResponse>>.FailureResponse("Category not found");
                }

                var products = await _unitOfWork.PartProducts.AsQueryable()
                    .Include(p => p.Category)
                    .Where(p => p.PartCategoryId == categoryId && p.DeletedAt == null)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var responses = products.Select(p => p.ToResponse()).ToList();

                return ApiResponse<List<PartProductResponse>>.SuccessResponse(
                    responses,
                    $"Retrieved {responses.Count} products successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category ID: {CategoryId}", categoryId);
                return ApiResponse<List<PartProductResponse>>.FailureResponse("Error retrieving products");
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
                    return ApiResponse<PartProductResponse>.FailureResponse("Product not found");
                }

                return ApiResponse<PartProductResponse>.SuccessResponse(
                    product.ToResponse(),
                    "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part product with ID: {ProductId}", id);
                return ApiResponse<PartProductResponse>.FailureResponse("Error retrieving product");
            }
        }
    }
}
