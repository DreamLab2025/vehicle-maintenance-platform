using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.Notifications;

namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationListItemDto>>> GetInAppNotificationsForUserAsync(
            Guid userId,
            PaginationRequest request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<NotificationDetailDto>> GetNotificationDetailForUserAsync(
            Guid userId,
            Guid notificationId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<NotificationStatusDto>> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<ApiResponse<int>> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> SoftDeleteByIdAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

        /// <summary>Marks the InApp delivery record as Sent and persists the notification metadata. Called by consumers after SignalR delivery.</summary>
        Task MarkInAppDeliveredAsync(Guid notificationId, Guid userId, IReadOnlyDictionary<string, object?> metadata, CancellationToken cancellationToken = default);
    }
}
