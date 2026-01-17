using Verendar.Common.Contracts;

namespace Verender.Identity.Contracts.Events;

public class UserRegisteredEvent : BaseEvent
{
    public override string EventType => "identity.user.registered.v1";
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberVerified { get; set; } = false;
    public string? Email { get; set; }
    public bool EmailVerified { get; set; } = false;
}
