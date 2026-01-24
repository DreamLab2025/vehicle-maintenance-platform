using System;
using System.Text.Json;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class NotificationMappings
{
    public static Domain.Entities.Notification OtpRequestedToNotificationEntity(
            this OtpRequestedEvent message,
            string title,
            string content,
            Domain.Enums.NotificationType type,
            bool isFallback = false)
    {
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = type,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new
            {
                OtpType = message.Type,
                Expiry = message.ExpiryTime,
                IsFallback = isFallback,
            })
        };
    }

    public static Domain.Entities.Notification UserRegisteredToNotificationEntity(
        this UserRegisteredEvent message,
        string title,
        string content,
        Domain.Enums.NotificationType type,
        bool isFallback = false)
    {
        return new Domain.Entities.Notification
        {
            UserId = message.UserId,
            Title = title,
            Message = content,
            NotificationType = type,
            Priority = isFallback ? NotificationPriority.Low : NotificationPriority.Medium,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new
            {
                IsFallback = isFallback,
            })
        };
    }

    public static NotificationDelivery CreateDelivery(
        this Domain.Entities.Notification notification,
        string? recipientAddress,
        NotificationChannel channel)
    {
        return new NotificationDelivery
        {
            NotificationId = notification.Id,
            Channel = channel,
            RecipientAddress = recipientAddress,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            MaxRetries = 3
        };
    }
}
