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
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Mã danh mục phụ tùng đã tồn tại");
                }

                var category = request.ToEntity();
                await _unitOfWork.PartCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created part category {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Tạo danh mục phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part category {CategoryName}", request.Name);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Lỗi khi tạo danh mục phụ tùng");
            }
        }

        public async Task<ApiResponse<PartCategoryResponse>> UpdateCategoryAsync(Guid id, PartCategoryRequest request)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Không tìm thấy danh mục phụ tùng");
                }

                var existingByCode = await _unitOfWork.PartCategories.GetByCodeAsync(request.Code);
                if (existingByCode != null && existingByCode.Id != id)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Mã danh mục phụ tùng đã tồn tại");
                }

                category.UpdateEntity(request);
                await _unitOfWork.PartCategories.UpdateAsync(id, category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated part category {CategoryName} (ID: {CategoryId})", category.Name, id);

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Cập nhật danh mục phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part category with ID: {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Lỗi khi cập nhật danh mục phụ tùng");
            }
        }

        public async Task<ApiResponse<string>> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy danh mục phụ tùng");
                }

                await _unitOfWork.PartCategories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted part category {CategoryName} (ID: {CategoryId})", category.Name, id);

                return ApiResponse<string>.SuccessResponse("Đã xóa", "Xóa danh mục phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part category with ID: {CategoryId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa danh mục phụ tùng");
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
                    "Lấy danh sách danh mục phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all part categories");
                return ApiResponse<List<PartCategoryResponse>>.FailureResponse("Lỗi khi lấy danh sách danh mục phụ tùng");
            }
        }

        public async Task<ApiResponse<PartCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.PartCategories.GetByIdAsync(id);
                if (category == null || category.DeletedAt != null)
                {
                    return ApiResponse<PartCategoryResponse>.FailureResponse("Không tìm thấy danh mục phụ tùng");
                }

                return ApiResponse<PartCategoryResponse>.SuccessResponse(
                    category.ToResponse(),
                    "Lấy thông tin danh mục phụ tùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part category with ID: {CategoryId}", id);
                return ApiResponse<PartCategoryResponse>.FailureResponse("Lỗi khi lấy thông tin danh mục phụ tùng");
            }
        }

        public async Task<ApiResponse<List<PartCategoryResponse>>> GetCategoriesByVehicleDeclaredPartsAsync(Guid userVehicleId)
        {
            try
            {
                var trackings = await _unitOfWork.VehiclePartTrackings.GetDeclaredByUserVehicleIdAsync(userVehicleId);
                var categories = trackings
                    .Where(t => t.PartCategory != null && t.PartCategory.DeletedAt == null)
                    .Select(t => t.PartCategory!)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.CreatedAt)
                    .Select(c => c.ToResponse())
                    .ToList();

                return ApiResponse<List<PartCategoryResponse>>.SuccessResponse(
                    categories,
                    "Lấy danh mục phụ tùng đã khai báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part categories by vehicle declared parts for UserVehicleId: {UserVehicleId}", userVehicleId);
                return ApiResponse<List<PartCategoryResponse>>.FailureResponse("Lỗi khi lấy danh mục phụ tùng đã khai báo theo xe");
            }
        }

        public async Task<ApiResponse<List<ReminderWithPartCategoryDto>>> GetRemindersByCategoryCodeAsync(Guid userId, Guid userVehicleId, string partCategoryCode)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<List<ReminderWithPartCategoryDto>>.FailureResponse("Không tìm thấy xe");
                }

                if (string.IsNullOrWhiteSpace(partCategoryCode))
                {
                    return ApiResponse<List<ReminderWithPartCategoryDto>>.FailureResponse("Part category code không hợp lệ");
                }

                var reminders = (await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(userVehicleId))
                    .Where(r => string.Equals(r.PartTracking?.PartCategory?.Code, partCategoryCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();
                var dtos = reminders.Select(r => r.ToReminderWithPartCategoryDto(vehicle.CurrentOdometer)).ToList();

                return ApiResponse<List<ReminderWithPartCategoryDto>>.SuccessResponse(
                    dtos,
                    "Lấy lịch sử nhắc bảo trì theo danh mục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reminders by category {PartCategoryCode} for vehicle {UserVehicleId}", partCategoryCode, userVehicleId);
                return ApiResponse<List<ReminderWithPartCategoryDto>>.FailureResponse("Lỗi khi lấy lịch sử nhắc bảo trì theo danh mục");
            }
        }
    }
}
