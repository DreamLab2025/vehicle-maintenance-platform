using MassTransit;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;

namespace Verendar.Notification.Application.Consumers
{
    public class BookingCreatedConsumer(
        ILogger<BookingCreatedConsumer> logger,
        IUnitOfWork unitOfWork,
        IInAppNotificationService inAppNotificationService,
        INotificationService notificationService) : IConsumer<BookingCreatedEvent>
    {
        private readonly ILogger<BookingCreatedConsumer> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IInAppNotificationService _inAppNotificationService = inAppNotificationService;
        private readonly INotificationService _notificationService = notificationService;

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

            _logger.LogDebug(
                "Processing BookingCreated — MessageId: {MessageId}, BookingId: {BookingId}, UserId: {UserId}",
                messageId, message.BookingId, message.UserId);

            try
            {
                var title = "Đặt lịch thành công";
                var content =
                    $"Bạn đã đặt lịch tại {message.BranchName} — {message.ProductName}. "
                    + $"Lịch: {message.ScheduledAt:dd/MM/yyyy HH:mm} (UTC).";

                var notification = message.BookingCreatedToNotificationEntity(title, content);
                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.NotificationDeliveries.AddAsync(
                    notification.CreateDelivery(message.UserId.ToString(), NotificationChannel.InApp));
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                var metadata = new Dictionary<string, object?>
                {
                    ["bookingId"] = message.BookingId,
                    ["entityType"] = "Booking"
                };

                var inAppPayload = new InAppNotificationPayload
                {
                    Title = title,
                    Message = content,
                    Metadata = metadata
                };

                await _inAppNotificationService.SendAsync(message.UserId, inAppPayload, context.CancellationToken);
                await _notificationService.MarkInAppDeliveredAsync(
                    notification.Id, message.UserId, inAppPayload.Metadata, context.CancellationToken);

                _logger.LogInformation(
                    "BookingCreated processed — MessageId: {MessageId}, BookingId: {BookingId}, NotificationId: {NotificationId}",
                    messageId, message.BookingId, notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BookingCreated — MessageId: {MessageId}, BookingId: {BookingId}",
                    messageId, message.BookingId);
            }
        }
    }
}
