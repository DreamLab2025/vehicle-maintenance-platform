using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;

namespace Verendar.Notification.Application.Services.Implements
{
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

        public async Task<ApiResponse<NotificationStatusDto>> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var unReadCount = await unitOfWork.Notifications.GetUnreadCountByUserIdAsync(userId, cancellationToken);
            return ApiResponse<NotificationStatusDto>.SuccessResponse(new NotificationStatusDto { UnReadCount = unReadCount });
        }

        public async Task<ApiResponse<int>> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var unread = await unitOfWork.Notifications.GetUnreadByUserIdWithInAppAsync(userId, cancellationToken);
            var readAt = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = readAt;
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.SuccessResponse(unread.Count, $"Đã đánh dấu {unread.Count} thông báo là đã đọc.");
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await unitOfWork.Notifications.GetByIdAndUserIdAsync(notificationId, userId, cancellationToken);
            if (notification == null)
                return ApiResponse<bool>.FailureResponse("Không tìm thấy thông báo.");

            if (notification.IsRead)
                return ApiResponse<bool>.SuccessResponse(true, "Thông báo đã được đánh dấu đọc trước đó.");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await unitOfWork.Notifications.UpdateAsync(notification.Id, notification);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true, "Đã đánh dấu thông báo là đã đọc.");
        }

        public async Task<ApiResponse<bool>> SoftDeleteByIdAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await unitOfWork.Notifications.GetByIdAndUserIdAsync(notificationId, userId, cancellationToken);
            if (notification == null)
                return ApiResponse<bool>.FailureResponse("Không tìm thấy thông báo.");

            notification.DeletedAt = DateTime.UtcNow;
            notification.DeletedBy = userId;
            await unitOfWork.Notifications.UpdateAsync(notification.Id, notification);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true, "Đã xóa thông báo.");
        }
    }
}
