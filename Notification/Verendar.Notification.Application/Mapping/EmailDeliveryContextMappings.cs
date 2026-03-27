using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Email;
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
                Title = title,
                Message = messageContent,
                NotificationType = notificationType,
                TemplateModel = new OtpEmailModel
                {
                    OtpCode = message.Otp,
                    ExpiryMinutes = (int)Math.Ceiling(expiryMinutes),
                    ExpiryTime = message.ExpiryTime,
                    OtpType = message.Type.ToString()
                },
                Metadata = new Dictionary<string, object>
                {
                    { NotificationConstants.MetadataKeys.TemplateKey, NotificationConstants.TemplateKeys.Otp }
                }
            };
        }

        public static WelcomeEmailModel ToWelcomeEmailModel(this UserRegisteredEvent message) => new()
        {
            FullName = message.FullName,
            UserName = message.FullName,
            UserEmail = message.Email,
            RegistrationDate = message.RegistrationDate
        };

        public static MemberAccountCreatedEmailModel ToMemberAccountCreatedEmailModel(this MemberAccountCreatedEvent message) => new()
        {
            FullName = message.FullName,
            UserName = message.FullName,
            UserEmail = message.Email,
            TempPassword = message.TempPassword,
            Role = message.Role
        };

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
                Title = title,
                Message = messageContent,
                NotificationType = notificationType,
                TemplateModel = templateModel,
                Metadata = new Dictionary<string, object>
                {
                    { NotificationConstants.MetadataKeys.TemplateKey, templateKey }
                }
            };
        }
    }
}
