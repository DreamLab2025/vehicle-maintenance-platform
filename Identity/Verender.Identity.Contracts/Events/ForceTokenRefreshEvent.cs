using Verendar.Common.Contracts;

namespace Verender.Identity.Contracts.Events
{
    public class ForceTokenRefreshEvent : BaseEvent
    {
        public override string EventType => "identity.token.force-refresh.v1";
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
