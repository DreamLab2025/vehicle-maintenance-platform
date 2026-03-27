using MassTransit;
using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Dtos.InApp;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;

namespace Verendar.Notification.Application.Consumers;

public class BookingCompletedEventConsumer(
    ILogger<BookingCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork,
    IInAppNotificationService inAppNotificationService,
    INotificationService notificationService) : IConsumer<BookingCompletedEvent>
{
    private readonly ILogger<BookingCompletedEventConsumer> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IInAppNotificationService _inAppNotificationService = inAppNotificationService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task Consume(ConsumeContext<BookingCompletedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogDebug(
            "Processing BookingCompleted — MessageId:{MessageId} BookingId:{BookingId} UserId:{UserId}",
            messageId, message.BookingId, message.UserId);

        try
        {
            var title = "Bảo dưỡng hoàn tất";
            var content = $"Xe của bạn vừa được bảo dưỡng tại {message.BranchName}. "
                + "Vui lòng xác nhận để cập nhật lịch sử bảo dưỡng và vòng đời phụ tùng.";

            var notification = message.BookingCompletedToNotificationEntity(title, content);
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.NotificationDeliveries.AddAsync(
                notification.CreateDelivery(message.UserId.ToString(), NotificationChannel.InApp));
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            var metadata = new Dictionary<string, object?>
            {
                ["bookingId"] = message.BookingId,
                ["userVehicleId"] = message.UserVehicleId,
                ["entityType"] = "MaintenanceProposal"
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
                "BookingCompleted notification sent — MessageId:{MessageId} BookingId:{BookingId} NotificationId:{NotificationId}",
                messageId, message.BookingId, notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing BookingCompleted — MessageId:{MessageId} BookingId:{BookingId}",
                messageId, message.BookingId);
        }
    }
}
