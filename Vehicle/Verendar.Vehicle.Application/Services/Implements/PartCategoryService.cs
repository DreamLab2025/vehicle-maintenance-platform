using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartCategoryService(ILogger<PartCategoryService> logger, IUnitOfWork unitOfWork) : IPartCategoryService
    {
        private readonly ILogger<PartCategoryService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request)
        {
            try
            {
                if (await _unitOfWork.PartCategories.GetByCodeAsync(request.Code) != null)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Category code already exists");
                }

                var category = request.ToEntity();
                await _unitOfWork.PartCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created part category {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part category {CategoryName}", request.Name);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Error creating category");
            }
        }

        public async Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Category not found");
                }

                var existingByCode = await _unitOfWork.PartCategories.GetByCodeAsync(request.Code);
                if (existingByCode != null && existingByCode.Id != id)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Category code already exists");
                }

                category.UpdateEntity(request);
                await _unitOfWork.PartCategories.UpdateAsync(id, category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated part category {CategoryName} (ID: {CategoryId})", category.Name, id);

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part category with ID: {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Error updating category");
            }
        }

        public async Task<ApiResponse<string>> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Category not found");
                }

                await _unitOfWork.PartCategories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted part category {CategoryName} (ID: {CategoryId})", category.Name, id);

                return ApiResponse<string>.SuccessResponse("Deleted", "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part category with ID: {CategoryId}", id);
                return ApiResponse<string>.FailureResponse("Error deleting category");
            }
        }

        public async Task<ApiResponse<List<PartCategoryResponse>>> GetAllCategoriesAsync(PaginationRequest paginationRequest)
        {
            try
            {
                var query = _unitOfWork.PartCategories.AsQueryable()
                    .Where(c => c.DeletedAt == null);

                var totalCount = await query.CountAsync();

                query = paginationRequest.IsDescending.HasValue && paginationRequest.IsDescending.Value
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt);

                var items = await query
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .ToListAsync();

                var responses = items.Select(c => c.ToResponse()).ToList();

                return ApiResponse<List<PartCategoryResponse>>.SuccessPagedResponse(
                    responses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all part categories");
                return ApiResponse<List<PartCategoryResponse>>.FailureResponse("Error retrieving categories");
            }
        }

        public async Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Category not found");
                }

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part category with ID: {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Error retrieving category");
            }
        }
    }
}
