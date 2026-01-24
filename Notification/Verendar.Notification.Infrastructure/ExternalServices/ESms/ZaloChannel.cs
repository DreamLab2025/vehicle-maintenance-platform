using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.ExternalServices.ESms;

public class ZaloChannel(IESmsService esmsService, ILogger<ZaloChannel> logger) : INotificationChannel
{
    private readonly IESmsService _esmsService = esmsService;
    private readonly ILogger<ZaloChannel> _logger = logger;
    public NotificationChannel ChannelType => NotificationChannel.ZALO;

    public async Task<ChannelDeliveryResult> SendAsync(NotificationDeliveryContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(context.RecipientPhone))
            {
                _logger.LogError("Recipient phone is required");
                return ChannelDeliveryResult.Failed("Số điện thoại không hợp lệ");
            }

            if (string.IsNullOrEmpty(context.ZaloTemplateId))
            {
                _logger.LogError("Zalo template ID is required");
                return ChannelDeliveryResult.Failed("Mã template Zalo không hợp lệ");
            }

            var result = await _esmsService.SendZaloZnsAsync(
                context.RecipientPhone,
                context.ZaloTemplateId,
                context.TemplateParameters ?? new Dictionary<string, string>(),
                context.NotificationId.ToString()
            );

            if (result.IsSuccess)
            {
                return ChannelDeliveryResult.Success(result.SmsId);
            }

            return ChannelDeliveryResult.Failed(
                $"Zalo Error: {result.CodeResult} - {result.ErrorMessage}",
                shouldRetry: false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zalo delivery failed for notification {NotificationId}",
                context.NotificationId);
            return ChannelDeliveryResult.Failed(ex.Message, shouldRetry: false);
        }
    }
}
