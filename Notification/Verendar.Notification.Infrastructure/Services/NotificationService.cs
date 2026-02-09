using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;

namespace Verendar.Notification.Infrastructure.Services;

public class NotificationService(IUnitOfWork unitOfWork) : INotificationService
{
    public async Task<ApiResponse<NotificationDetailDto>> GetNotificationDetailForUserAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await unitOfWork.Notifications.GetByIdAndUserIdAsync(notificationId, userId, cancellationToken);
        if (notification == null)
            return ApiResponse<NotificationDetailDto>.FailureResponse("Không tìm thấy thông báo.");

        var hasInApp = await unitOfWork.NotificationDeliveries.FindOneAsync(d =>
            d.NotificationId == notificationId && d.Channel == NotificationChannel.InApp) != null;
        if (!hasInApp)
            return ApiResponse<NotificationDetailDto>.FailureResponse("Thông báo không có trong app.");

        return ApiResponse<NotificationDetailDto>.SuccessResponse(notification.ToDetailDto());
    }

    public async Task<ApiResponse<List<NotificationListItemDto>>> GetInAppNotificationsForUserAsync(
        Guid userId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await unitOfWork.Notifications.GetByUserIdWithInAppChannelPagedAsync(
                userId, request.PageNumber, request.PageSize, cancellationToken);

            var dtos = items.Select(n => n.ToListItemDto()).ToList();
            return ApiResponse<List<NotificationListItemDto>>.SuccessPagedResponse(
                dtos, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception)
        {
            return ApiResponse<List<NotificationListItemDto>>.FailureResponse("Lỗi khi lấy danh sách thông báo.");
        }
    }
}
