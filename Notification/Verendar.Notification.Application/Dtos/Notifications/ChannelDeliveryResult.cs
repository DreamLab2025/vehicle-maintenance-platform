using System;

namespace Verendar.Notification.Application.Dtos.Notifications;

public class ChannelDeliveryResult
{
    public bool IsSuccess { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShouldRetry { get; set; }

    public static ChannelDeliveryResult Success(string? externalId = null)
    {
        return new ChannelDeliveryResult
        {
            IsSuccess = true,
            ExternalId = externalId
        };
    }

    public static ChannelDeliveryResult Failed(string errorMessage, bool shouldRetry = false)
    {
        return new ChannelDeliveryResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ShouldRetry = shouldRetry
        };
    }
}
