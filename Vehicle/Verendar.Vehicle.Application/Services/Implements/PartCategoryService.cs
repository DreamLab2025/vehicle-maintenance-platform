using MassTransit;
using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartCategoryService(
        ILogger<PartCategoryService> logger,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint) : IPartCategoryService
    {
        private readonly ILogger<PartCategoryService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<ApiResponse<PartCategoryResponse>> CreateCategoryAsync(PartCategoryRequest request)
        {
            var category = request.ToEntity();
            category.Slug = await SlugUtils.EnsureUniqueAsync(
                SlugUtils.ToSlug(request.Name, 50),
                async s => (await _unitOfWork.PartCategories.GetBySlugAsync(s)) != null,
                maxLength: 50);

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

            var previousIconMediaFileId = category.IconMediaFileId;

            category.UpdateEntity(request);
            await _unitOfWork.PartCategories.UpdateAsync(id, category);
            await _unitOfWork.SaveChangesAsync();

            if (previousIconMediaFileId.HasValue && previousIconMediaFileId != request.IconMediaFileId)
            {
                await TryPublishPartCategoryIconSupersededAsync(id, previousIconMediaFileId);
            }

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

            var supersededIconMediaFileId = category.IconMediaFileId;

            await _unitOfWork.PartCategories.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await TryPublishPartCategoryIconSupersededAsync(id, supersededIconMediaFileId);

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

        public async Task<ApiResponse<List<ReminderDetailDto>>> GetRemindersByCategorySlugAsync(Guid userId, Guid userVehicleId, string partCategorySlug)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetRemindersByCategorySlug: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<ReminderDetailDto>>.NotFoundResponse("Không tìm thấy xe");
            }

            if (string.IsNullOrWhiteSpace(partCategorySlug))
            {
                _logger.LogWarning("GetRemindersByCategorySlug: empty slug vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<ReminderDetailDto>>.FailureResponse("Slug danh mục phụ tùng không hợp lệ");
            }

            var reminders = (await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(userVehicleId))
                .Where(r => string.Equals(r.TrackingCycle?.PartTracking?.PartCategory?.Slug, partCategorySlug.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            var dtos = reminders.Select(r => r.ToReminderDetailDto(vehicle.CurrentOdometer)).ToList();

            return ApiResponse<List<ReminderDetailDto>>.SuccessResponse(
                dtos,
                "Lấy lịch sử nhắc bảo trì theo danh mục thành công");
        }

        private async Task TryPublishPartCategoryIconSupersededAsync(Guid partCategoryId, Guid? supersededMediaFileId)
        {
            if (!supersededMediaFileId.HasValue || supersededMediaFileId.Value == Guid.Empty)
            {
                return;
            }

            try
            {
                await _publishEndpoint.Publish(new PartCategoryIconMediaSupersededEvent
                {
                    PartCategoryId = partCategoryId,
                    SupersededMediaFileId = supersededMediaFileId.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to publish PartCategoryIconMediaSuperseded for category {CategoryId}, media {MediaFileId}",
                    partCategoryId, supersededMediaFileId);
            }
        }
    }
}
