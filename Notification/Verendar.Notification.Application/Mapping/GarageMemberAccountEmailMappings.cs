using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Email;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Mapping;

public static class GarageMemberAccountEmailMappings
{
    public static MemberAccountCreatedEmailModel ToMemberAccountCreatedEmailModel(
        this MemberAccountCreatedEvent message,
        string loginUrl,
        string loginCtaText)
        => new()
        {
            UserName = message.Email,
            Title = NotificationConstants.Titles.MemberAccountCreated,
            DisplayName = message.FullName,
            Role = message.Role,
            TempPassword = message.TempPassword,
            CtaUrl = loginUrl,
            CtaText = loginCtaText
        };
}
