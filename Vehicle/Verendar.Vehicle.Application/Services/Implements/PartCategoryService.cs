using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartCategoryService(ILogger<PartCategoryService> logger, IUnitOfWork unitOfWork) : IPartCategoryService
    {
        private readonly ILogger<PartCategoryService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request)
        {
            if (await _unitOfWork.PartCategories.GetByCodeAsync(request.Code) != null)
            {
                _logger.LogWarning("CreateCategory: code exists {Code}", request.Code);
                return ApiResponse<PartCategoryResponse>.ConflictResponse("Mã danh mục phụ tùng đã tồn tại");
            }

            var category = request.ToEntity();
            await _unitOfWork.PartCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<PartCategoryResponse>.CreatedResponse(
                category.ToResponse(),
                "Tạo danh mục phụ tùng thành công");
        }

        public async Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request)
        {
            var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("UpdateCategory: not found {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            var existingByCode = await _unitOfWork.PartCategories.GetByCodeAsync(request.Code);
            if (existingByCode != null && existingByCode.Id != id)
            {
                _logger.LogWarning("UpdateCategory: code conflict {Code} for {CategoryId}", request.Code, id);
                return ApiResponse<PartCategoryResponse>.ConflictResponse("Mã danh mục phụ tùng đã tồn tại");
            }

            category.UpdateEntity(request);
            await _unitOfWork.PartCategories.UpdateAsync(id, category);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<PartCategoryResponse>.SuccessResponse(
                category.ToResponse(),
                "Cập nhật danh mục phụ tùng thành công");
        }

        public async Task<ApiResponse<string>> DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("DeleteCategory: not found {CategoryId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            await _unitOfWork.PartCategories.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Đã xóa", "Xóa danh mục phụ tùng thành công");
        }

        public async Task<ApiResponse<List<PartCategorySummary>>> GetAllCategoriesAsync(PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            var query = _unitOfWork.PartCategories.AsQueryable();

            var totalCount = await query.CountAsync();

            query = paginationRequest.IsDescending.HasValue && paginationRequest.IsDescending.Value
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt);

            var items = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var summaries = items.Select(c => c.ToSummary()).ToList();

            return ApiResponse<List<PartCategorySummary>>.SuccessPagedResponse(
                summaries,
                totalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy danh sách danh mục phụ tùng thành công");
        }

        public async Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("GetCategoryById: not found {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            return ApiResponse<PartCategoryResponse>.SuccessResponse(
                category.ToResponse(),
                "Lấy thông tin danh mục phụ tùng thành công");
        }

        public async Task<ApiResponse<List<PartCategorySummary>>> GetCategoriesByVehicleDeclaredPartsAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);
            if (vehicle == null)
            {
                _logger.LogWarning("GetCategoriesByVehicleDeclaredParts: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<PartCategorySummary>>.NotFoundResponse("Không tìm thấy xe");
            }

            var trackings = await _unitOfWork.PartTrackings.GetDeclaredByUserVehicleIdAsync(userVehicleId);
            var categories = trackings
                .Where(t => t.PartCategory != null)
                .Select(t => t.PartCategory!)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CreatedAt)
                .Select(c => c.ToSummary())
                .ToList();

            return ApiResponse<List<PartCategorySummary>>.SuccessResponse(
                categories,
                "Lấy danh mục phụ tùng đã khai báo thành công");
        }

        public async Task<ApiResponse<List<ReminderDetailDto>>> GetRemindersByCategoryCodeAsync(Guid userId, Guid userVehicleId, string partCategoryCode)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetRemindersByCategoryCode: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<ReminderDetailDto>>.NotFoundResponse("Không tìm thấy xe");
            }

            if (string.IsNullOrWhiteSpace(partCategoryCode))
            {
                _logger.LogWarning("GetRemindersByCategoryCode: empty code vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<ReminderDetailDto>>.FailureResponse("Part category code không hợp lệ");
            }

            var reminders = (await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(userVehicleId))
                .Where(r => string.Equals(r.TrackingCycle?.PartTracking?.PartCategory?.Code, partCategoryCode.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            var dtos = reminders.Select(r => r.ToReminderDetailDto(vehicle.CurrentOdometer)).ToList();

            return ApiResponse<List<ReminderDetailDto>>.SuccessResponse(
                dtos,
                "Lấy lịch sử nhắc bảo trì theo danh mục thành công");
        }
    }
}
