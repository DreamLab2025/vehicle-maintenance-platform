using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Domain.Enums;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Mapping
{
    public static class EmailDeliveryContextMappings
    {
        public static NotificationDeliveryContext ToDeliveryContext(
            this OtpRequestedEvent message,
            Guid notificationId,
            string title,
            string messageContent,
            NotificationType notificationType,
            double expiryMinutes)
        {
            return new NotificationDeliveryContext
            {
                NotificationId = notificationId,
                RecipientEmail = message.TargetValue,
                RecipientPhone = null,
                Title = title,
                Message = messageContent,
                NotificationType = notificationType,
                TemplateParameters = new Dictionary<string, string>
                {
                    { "OTP", message.Otp },
                    { "OtpCode", message.Otp },
                    { "ExpiryMinutes", expiryMinutes.ToString() },
                    { "ExpiryTime", message.ExpiryTime.ToString(NotificationConstants.DateFormats.DateTime) },
                    { "Type", message.Type.ToString() }
                },
                Metadata = new Dictionary<string, object> { { NotificationConstants.MetadataKeys.TemplateKey, NotificationConstants.TemplateKeys.Otp } }
            };
        }

        public static NotificationDeliveryContext ToDeliveryContext(
            Guid notificationId,
            string recipientEmail,
            string title,
            string messageContent,
            NotificationType notificationType,
            object templateModel,
            string templateKey)
        {
            return new NotificationDeliveryContext
            {
                NotificationId = notificationId,
                RecipientEmail = recipientEmail,
                RecipientPhone = null,
                Title = title,
                Message = messageContent,
                NotificationType = notificationType,
                Metadata = new Dictionary<string, object> { { NotificationConstants.MetadataKeys.TemplateKey, templateKey } },
                TemplateModel = templateModel
            };
        }
    }
}
