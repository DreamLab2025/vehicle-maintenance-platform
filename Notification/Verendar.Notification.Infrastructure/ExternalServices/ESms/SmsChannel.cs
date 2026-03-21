using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Verendar.Notification.Infrastructure.ExternalServices.ESms
{
    public class SmsChannel(IESmsService esmsService, ILogger<SmsChannel> logger) : INotificationChannel
    {
        private readonly IESmsService _esmsService = esmsService;
        private readonly ILogger<SmsChannel> _logger = logger;
        public NotificationChannel ChannelType => NotificationChannel.SMS;
    
        public async Task<ChannelDeliveryResult> SendAsync(NotificationDeliveryContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(context.RecipientPhone))
                {
                    _logger.LogError("Recipient phone is required");
                    return ChannelDeliveryResult.Failed("Số điện thoại không hợp lệ");
                }

                _logger.LogInformation("Sending SMS to {Phone}, RequestId: {RequestId}", context.RecipientPhone, context.NotificationId.ToString());

                var result = await _esmsService.SendSmsAsync(
                    context.RecipientPhone,
                    context.Message,
                    context.NotificationId.ToString()
                );

                if (result.IsSuccess)
                {
                    return ChannelDeliveryResult.Success(result.SmsId);
                }

                return ChannelDeliveryResult.Failed(
                    $"eSMS Error: {result.CodeResult} - {result.ErrorMessage}",
                    shouldRetry: ShouldRetry(result.CodeResult)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS delivery failed for notification {NotificationId}",
                                    context.NotificationId);
                return ChannelDeliveryResult.Failed(ex.Message, shouldRetry: true);
            }
        }

        private static bool ShouldRetry(string errorCode)
        {
            return errorCode switch
            {
                "104" => false,
                "118" => false,
                "119" => false,
                _ => true
            };
        }
    }
}
