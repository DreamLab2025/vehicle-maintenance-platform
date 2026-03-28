using Verender.Identity.Contracts.Events;
namespace Verendar.Notification.Application.Mapping
{
    public static class NotificationPreferenceMappings
    {
        public static Domain.Entities.NotificationPreference UserRegisteredToPreferenceEntity(
            this UserRegisteredEvent message,
            bool isEnabled = true)
        {
            return new Domain.Entities.NotificationPreference
            {
                UserId = message.UserId,
                PhoneNumber = message.PhoneNumber,
                PhoneNumberVerified = message.PhoneNumberVerified,
                Email = message.Email,
                EmailVerified = message.EmailVerified,
                InAppEnabled = isEnabled
            };
        }
    }
}
