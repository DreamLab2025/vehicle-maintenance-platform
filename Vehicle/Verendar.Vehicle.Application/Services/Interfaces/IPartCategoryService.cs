namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartCategoryService
    {
        Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request);
        Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request);
        Task<ApiResponse<string>> DeleteCategoryAsync(Guid id);
        Task<ApiResponse<List<PartCategoryResponse>>> GetAllCategoriesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id);
        /// <summary>Lấy danh sách category của các phụ tùng đã khai báo theo user vehicle.</summary>
        Task<ApiResponse<List<PartCategoryResponse>>> GetCategoriesByVehicleDeclaredPartsAsync(Guid userId, Guid userVehicleId);
        /// <summary>Lấy toàn bộ reminder (current + lịch sử) của xe theo part category code.</summary>
        Task<ApiResponse<List<ReminderWithPartCategoryDto>>> GetRemindersByCategoryCodeAsync(Guid userId, Guid userVehicleId, string partCategoryCode);
    }
}
