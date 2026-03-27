using System.Text.Json;
using Verendar.Notification.Application.Clients;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Notification.Application.Services.Implements
{
    public class NotificationService(
        IUnitOfWork unitOfWork,
        IVehicleMaintenanceReminderLookupClient vehicleLookupClient,
        ILogger<NotificationService> logger) : INotificationService
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

            var dto = notification.ToDetailDto();

            if (string.Equals(notification.EntityType, "MaintenanceReminder", StringComparison.OrdinalIgnoreCase))
            {
                var reminderIds = ParseReminderIdsFromMetadata(notification.MetadataJson);
                if (reminderIds.Count > 0)
                {
                    try
                    {
                        var rows = await vehicleLookupClient.LookupAsync(notification.UserId, reminderIds, cancellationToken);
                        if (rows != null && rows.Count > 0)
                        {
                            var freshness = rows
                                .Select(r => BuildFreshness(r, notification.CreatedAt))
                                .ToList();
                            dto = dto with { MaintenanceReminderFreshness = freshness };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Vehicle reminder lookup failed for notification {NotificationId}", notificationId);
                    }
                }
            }

            return ApiResponse<NotificationDetailDto>.SuccessResponse(dto);
        }

        private static List<Guid> ParseReminderIdsFromMetadata(string? metadataJson)
        {
            if (string.IsNullOrEmpty(metadataJson))
                return [];

            try
            {
                using var doc = JsonDocument.Parse(metadataJson);
                if ((!doc.RootElement.TryGetProperty("items", out var itemsEl) && !doc.RootElement.TryGetProperty("Items", out itemsEl))
                    || itemsEl.ValueKind != JsonValueKind.Array)
                    return [];

                var list = new List<Guid>();
                foreach (var el in itemsEl.EnumerateArray())
                {
                    if (!TryGetGuidProperty(el, "reminderId", "ReminderId", out var g))
                        continue;
                    list.Add(g);
                }

                return list;
            }
            catch
            {
                return [];
            }
        }

        private static MaintenanceReminderFreshnessDto BuildFreshness(
            MaintenanceReminderLookupItemResponse row,
            DateTime notificationCreatedAtUtc)
        {
            var reminderIsActive = string.Equals(row.ReminderStatus, "Active", StringComparison.OrdinalIgnoreCase);
            var notificationDate = DateOnly.FromDateTime(notificationCreatedAtUtc.ToUniversalTime());
            var replacedAfter = row.LastReplacementDate.HasValue && row.LastReplacementDate.Value >= notificationDate;
            var trackingChanged = !reminderIsActive || replacedAfter;

            return new MaintenanceReminderFreshnessDto
            {
                ReminderId = row.ReminderId,
                PartTrackingId = row.PartTrackingId,
                ReminderIsActive = reminderIsActive,
                ReminderStatus = row.ReminderStatus,
                LastReplacementDate = row.LastReplacementDate,
                LastReplacementOdometer = row.LastReplacementOdometer,
                TrackingChangedSinceNotification = trackingChanged
            };
        }

        private static bool TryGetGuidProperty(JsonElement el, string camelName, string pascalName, out Guid value)
        {
            value = Guid.Empty;
            if (!el.TryGetProperty(camelName, out var prop) && !el.TryGetProperty(pascalName, out prop))
                return false;
            var s = prop.GetString();
            return s != null && Guid.TryParse(s, out value);
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
