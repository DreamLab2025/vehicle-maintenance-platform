namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartCategoryService
    {
        Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request);
        Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request);
        Task<ApiResponse<string>> DeleteCategoryAsync(Guid id);
        Task<ApiResponse<List<PartCategorySummary>>> GetAllCategoriesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id);
        Task<ApiResponse<List<PartCategorySummary>>> GetCategoriesByVehicleDeclaredPartsAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<List<ReminderDetailDto>>> GetRemindersByCategorySlugAsync(Guid userId, Guid userVehicleId, string partCategorySlug);
    }
}
