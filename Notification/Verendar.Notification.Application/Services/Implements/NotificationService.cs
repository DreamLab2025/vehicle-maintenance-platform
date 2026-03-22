using System.Text.Json;
using Verendar.Notification.Application.Mapping;

namespace Verendar.Notification.Application.Services.Implements
{
    public class NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger) : INotificationService
    {
        private readonly ILogger<NotificationService> _logger = logger;

        public async Task<ApiResponse<NotificationDetailDto>> GetNotificationDetailForUserAsync(
            Guid userId,
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var notification = await unitOfWork.Notifications.GetByIdAndUserIdAsync(notificationId, userId, cancellationToken);
            if (notification == null)
            {
                _logger.LogWarning("GetNotificationDetail: not found {NotificationId} for user {UserId}", notificationId, userId);
                return ApiResponse<NotificationDetailDto>.NotFoundResponse("Không tìm thấy thông báo.");
            }

            var hasInApp = await unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                d.NotificationId == notificationId && d.Channel == NotificationChannel.InApp) != null;
            if (!hasInApp)
            {
                _logger.LogWarning("GetNotificationDetail: no in-app delivery {NotificationId} user {UserId}", notificationId, userId);
                return ApiResponse<NotificationDetailDto>.FailureResponse("Thông báo không có trong app.");
            }

            return ApiResponse<NotificationDetailDto>.SuccessResponse(notification.ToDetailDto());
        }

        public async Task<ApiResponse<List<NotificationListItemDto>>> GetInAppNotificationsForUserAsync(
            Guid userId,
            PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            request.Normalize();
            var (items, totalCount) = await unitOfWork.Notifications.GetByUserIdWithInAppChannelPagedAsync(
                userId, request.PageNumber, request.PageSize, cancellationToken);

            var dtos = items.Select(n => n.ToListItemDto()).ToList();
            return ApiResponse<List<NotificationListItemDto>>.SuccessPagedResponse(
                dtos, totalCount, request.PageNumber, request.PageSize);
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
            {
                _logger.LogWarning("MarkAsRead: notification not found {NotificationId} user {UserId}", notificationId, userId);
                return ApiResponse<bool>.NotFoundResponse("Không tìm thấy thông báo.");
            }

            if (notification.IsRead)
            {
                _logger.LogWarning("MarkAsRead: already read {NotificationId} user {UserId}", notificationId, userId);
                return ApiResponse<bool>.SuccessResponse(true, "Thông báo đã được đánh dấu đọc trước đó.");
            }

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
            {
                _logger.LogWarning("SoftDeleteNotification: not found {NotificationId} user {UserId}", notificationId, userId);
                return ApiResponse<bool>.NotFoundResponse("Không tìm thấy thông báo.");
            }

            notification.DeletedAt = DateTime.UtcNow;
            notification.DeletedBy = userId;
            await unitOfWork.Notifications.UpdateAsync(notification.Id, notification);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true, "Đã xóa thông báo.");
        }

        public async Task MarkInAppDeliveredAsync(Guid notificationId, Guid userId, IReadOnlyDictionary<string, object?> metadata, CancellationToken cancellationToken = default)
        {
            var notification = await unitOfWork.Notifications.FindOneAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.MetadataJson = JsonSerializer.Serialize(metadata);
                await unitOfWork.Notifications.UpdateAsync(notification.Id, notification);
            }

            var delivery = await unitOfWork.NotificationDeliveries.FindOneAsync(d =>
                d.NotificationId == notificationId && d.Channel == NotificationChannel.InApp);
            if (delivery != null)
            {
                delivery.Status = NotificationStatus.Sent;
                delivery.SentAt = delivery.DeliveredAt = DateTime.UtcNow;
                await unitOfWork.NotificationDeliveries.UpdateAsync(delivery.Id, delivery);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
