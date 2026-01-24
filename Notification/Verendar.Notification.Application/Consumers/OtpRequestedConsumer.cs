using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Mapping;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public partial class OtpRequestedConsumer : IConsumer<OtpRequestedEvent>
{
    [GeneratedRegex("^0[0-9]{9}$")]
    private static partial Regex PhoneNumberRegex();

    private readonly ILogger<OtpRequestedConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChannelFactory _channelFactory;

    public OtpRequestedConsumer(
        ILogger<OtpRequestedConsumer> logger,
        IUnitOfWork unitOfWork,
        IChannelFactory channelFactory)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _channelFactory = channelFactory;
    }

    public async Task Consume(ConsumeContext<OtpRequestedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        
        _logger.LogDebug("Processing OTP request - MessageId: {MessageId}, UserId: {UserId}, Phone: {Phone}", 
            messageId, message.UserId, message.TargetValue);

        try
        {
            if (!ValidateMessage(message))
            {
                _logger.LogWarning("Invalid OTP request message - MessageId: {MessageId}, UserId: {UserId}", 
                    messageId, message.UserId);
                return;
            }

            // Idempotency check: Kiểm tra xem đã có notification cho OTP này chưa (trong 10 phút gần đây)
            var existingNotification = await _unitOfWork.Notifications
                .FindOneAsync(n => n.UserId == message.UserId &&
                                  n.CreatedAt > DateTime.UtcNow.AddMinutes(-10) &&
                                  n.Message.Contains(message.Otp));

            if (existingNotification != null)
            {
                _logger.LogWarning("Duplicate OTP request detected - MessageId: {MessageId}, UserId: {UserId}, ExistingNotificationId: {NotificationId}",
                    messageId, message.UserId, existingNotification.Id);
                return;
            }

            var templateCode = GetTemplateCode(message.Type);
            var notificationTemplate = await _unitOfWork.NotificationTemplates
                .FindOneAsync(nt => nt.Code == templateCode && nt.IsActive);

            if (notificationTemplate == null)
            {
                _logger.LogWarning("Notification template not found: {TemplateCode}", templateCode);

                await SendFallbackOtpAsync(message, messageId);
                return;
            }

            await SendOtpMessageAsync(message, notificationTemplate, messageId);

            _logger.LogInformation(
                "OTP SMS processed successfully - MessageId: {MessageId}, UserId: {UserId}, Phone: {Phone}",
                messageId, message.UserId, MaskPhoneNumber(message.TargetValue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP SMS - MessageId: {MessageId}, UserId: {UserId}", 
                messageId, message.UserId);
            
            // Không throw để tránh retry tạo duplicate notification
            // Chỉ log và return vì notification đã được lưu vào DB
        }
    }

    private async Task SendOtpMessageAsync(OtpRequestedEvent message, NotificationTemplate notificationTemplate, string? messageId = null)
    {
        var messageContent = ReplaceTemplatePlaceholders(notificationTemplate.MessageTemplate, message);
        var titleContent = ReplaceTemplatePlaceholders(notificationTemplate.TitleTemplate, message);

        // OTP luôn gửi qua SMS
        var targetChannel = NotificationChannel.SMS;

        // Tạo & Lưu DB (Pending)
        var notification = message.OtpRequestedToNotificationEntity(
            titleContent,
            messageContent,
            notificationTemplate.NotificationType,
            isFallback: false
        );
        var delivery = notification.CreateDelivery(message.TargetValue, targetChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        // Chuẩn bị Context cho SMS
        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientPhone = message.TargetValue,
            Title = titleContent,
            Message = messageContent,
            NotificationType = notificationTemplate.NotificationType,
            TemplateParameters = new Dictionary<string, string>
            {
                { "OTP", message.Otp },
                { "ExpiryMinutes", CalculateExpiryMinutes(message.ExpiryTime).ToString() },
                { "ExpiryTime", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy") },
                { "Type", message.Type.ToString() }
            },
            Metadata = new Dictionary<string, object> { { "IsFallback", false } }
        };

        // Gửi & Update Status (không throw exception nếu đã lưu vào DB)
        try
        {
            await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
        }
        catch (Exception ex)
        {
            // Notification đã được lưu vào DB, không throw để tránh retry
            _logger.LogError(ex, "Failed to send OTP after notification created - NotificationId: {NotificationId}, MessageId: {MessageId}",
                notification.Id, messageId);
        }
    }

    private async Task SendFallbackOtpAsync(OtpRequestedEvent message, string? messageId = null)
    {
        _logger.LogInformation("Sending fallback OTP message for UserId: {UserId}", message.UserId);

        var expiryMinutes = Math.Ceiling(CalculateExpiryMinutes(message.ExpiryTime));
        var fallbackMessage = $"Ma xac thuc OTP Verender cua ban la: {message.Otp}. " +
                              $"Hieu luc {expiryMinutes} phut. Khong chia se ma nay.";

        var targetChannel = NotificationChannel.SMS;

        // Tạo & Lưu DB
        var notification = message.OtpRequestedToNotificationEntity(
            "Ma xac thuc OTP",
            fallbackMessage,
            NotificationType.System,
            isFallback: true
        );
        var delivery = notification.CreateDelivery(message.TargetValue, targetChannel);

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        // Context cho Fallback SMS
        var deliveryContext = new NotificationDeliveryContext
        {
            NotificationId = notification.Id,
            RecipientPhone = message.TargetValue,
            Title = "Ma xac thuc OTP",
            Message = fallbackMessage,
            NotificationType = NotificationType.System,
            TemplateParameters = new Dictionary<string, string>
            {
                { "OTP", message.Otp },
                { "ExpiryMinutes", CalculateExpiryMinutes(message.ExpiryTime).ToString() },
                { "ExpiryTime", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy") },
                { "Type", message.Type.ToString() }
            },
            Metadata = new Dictionary<string, object> { { "IsFallback", true } }
        };

        try
        {
            await ExecuteSendAsync(targetChannel, deliveryContext, notification, delivery);
        }
        catch (Exception ex)
        {
            // Notification đã được lưu vào DB, không throw để tránh retry
            _logger.LogError(ex, "Failed to send fallback OTP after notification created - NotificationId: {NotificationId}, MessageId: {MessageId}",
                notification.Id, messageId);
        }
    }

    private async Task ExecuteSendAsync(
        NotificationChannel channel,
        NotificationDeliveryContext context,
        Domain.Entities.Notification notification,
        NotificationDelivery delivery)
    {
        try
        {
            var channelService = _channelFactory.GetChannel(channel);
            var result = await channelService.SendAsync(context);

            if (result.IsSuccess)
            {
                notification.Status = NotificationStatus.Sent;
                delivery.Status = NotificationStatus.Sent;
                delivery.SentAt = DateTime.UtcNow;
                delivery.DeliveredAt = DateTime.UtcNow;
                delivery.ExternalId = result.ExternalId;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                delivery.Status = NotificationStatus.Failed;
                delivery.ErrorMessage = result.ErrorMessage;
                delivery.RetryCount++;

                if (result.ShouldRetry) throw new Exception(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            delivery.Status = NotificationStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            // Không throw exception ở đây - đã được handle ở method gọi
            _logger.LogError(ex, "Error in ExecuteSendAsync - NotificationId: {NotificationId}", notification.Id);
        }
        finally
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private string? MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return "N/A";
        if (phoneNumber.Length <= 4) return "****";
        return new string('*', phoneNumber.Length - 3) + phoneNumber[^3..];
    }

    private double CalculateExpiryMinutes(DateTime expiryTime)
    {
        var timeSpan = expiryTime - DateTime.UtcNow;
        return timeSpan.TotalMinutes > 0 ? timeSpan.TotalMinutes : 0;
    }

    private string ReplaceTemplatePlaceholders(string template, OtpRequestedEvent message)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        var expiryMinutes = Math.Ceiling(CalculateExpiryMinutes(message.ExpiryTime));
        return template
            .Replace("{OTP}", message.Otp)
            .Replace("{Otp}", message.Otp)
            .Replace("{ExpiryMinutes}", expiryMinutes.ToString())
            .Replace("{ExpiryTime}", message.ExpiryTime.ToString("HH:mm dd/MM/yyyy"))
            .Replace("{Type}", message.Type.ToString());
    }

    private static string GetTemplateCode(OtpType type)
    {
        return "OTP_PHONE_VERIFICATION"; // Chỉ Phone
    }

    private bool ValidateMessage(OtpRequestedEvent message)
    {
        if (string.IsNullOrEmpty(message.TargetValue)) return false;
        if (message.ExpiryTime < DateTime.UtcNow) return false;
        if (!PhoneNumberRegex().IsMatch(message.TargetValue)) return false;
        return true;
    }
}