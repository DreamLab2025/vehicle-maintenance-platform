using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartCategoryService
    {
        Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request);
        Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request);
        Task<ApiResponse<string>> DeleteCategoryAsync(Guid id);
        Task<ApiResponse<List<PartCategoryResponse>>> GetAllCategoriesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id);
        Task<ApiResponse<List<PartCategoryResponse>>> GetCategoriesByVehicleTrackedPartsAsync(Guid userVehicleId);
    }
}
