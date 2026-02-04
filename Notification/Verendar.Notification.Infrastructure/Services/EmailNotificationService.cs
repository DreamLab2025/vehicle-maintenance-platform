using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Infrastructure.Services;

public class EmailNotificationService(
    IUnitOfWork unitOfWork,
    IChannelFactory channelFactory,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IChannelFactory _channelFactory = channelFactory;
    private readonly ILogger<EmailNotificationService> _logger = logger;

    private const NotificationChannel EmailChannel = NotificationChannel.EMAIL;
    private const string OtpEmailTitle = "Mã xác thực OTP";

    public async Task<bool> SendOtpEmailAsync(OtpRequestedEvent message, CancellationToken cancellationToken = default)
    {
        var expiryMinutes = Math.Ceiling(CalculateExpiryMinutes(message.ExpiryTime));
        var messageContent = $"Mã xác thực OTP Verendar của bạn là: {message.Otp}. Hiệu lực {expiryMinutes} phút. Không chia sẻ mã này.";

        var notification = message.OtpRequestedToNotificationEntity(
            OtpEmailTitle,
            messageContent,
            NotificationType.System,
            isFallback: false);
        var delivery = notification.CreateDelivery(message.TargetValue, EmailChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var context = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientEmail = message.TargetValue,
            RecipientPhone = null,
            Title = OtpEmailTitle,
            Message = messageContent,
            NotificationType = notification.NotificationType,
            TemplateParameters = new Dictionary<string, string>
            {
                { "OTP", message.Otp },
                { "OtpCode", message.Otp },
                { "ExpiryMinutes", CalculateExpiryMinutes(message.ExpiryTime).ToString() },
                { "ExpiryTime", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy") },
                { "Type", message.Type.ToString() }
            },
            Metadata = new Dictionary<string, object> { { "TemplateKey", "Otp" } }
        };

        try
        {
            var channelService = _channelFactory.GetChannel(EmailChannel);
            var result = await channelService.SendAsync(context);

            if (result.IsSuccess)
            {
                notification.Status = NotificationStatus.Sent;
                delivery.Status = NotificationStatus.Sent;
                delivery.SentAt = DateTime.UtcNow;
                delivery.DeliveredAt = DateTime.UtcNow;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                delivery.Status = NotificationStatus.Failed;
                delivery.ErrorMessage = result.ErrorMessage;
                delivery.RetryCount++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP email - NotificationId: {NotificationId}", notification.Id);
            notification.Status = NotificationStatus.Failed;
            delivery.Status = NotificationStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private static double CalculateExpiryMinutes(DateTime expiryTime)
    {
        var timeSpan = expiryTime - DateTime.UtcNow;
        return timeSpan.TotalMinutes > 0 ? timeSpan.TotalMinutes : 0;
    }
}
