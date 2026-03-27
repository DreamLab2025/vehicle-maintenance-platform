using Verendar.Common.Contracts;

namespace Verender.Identity.Contracts.Events
{
    public class MemberAccountCreatedEvent : BaseEvent
    {
        public override string EventType => "identity.member-account.created.v1";
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
