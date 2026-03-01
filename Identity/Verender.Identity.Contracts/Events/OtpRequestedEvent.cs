using Verendar.Common.Contracts;

namespace Verender.Identity.Contracts.Events
{
    public class OtpRequestedEvent : BaseEvent
    {
        public override string EventType => "identity.otp.requested.v1";
        public Guid UserId { get; set; }
        public string? TargetValue { get; set; } //Phone Number or Email
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public OtpType Type { get; set; } = OtpType.PhoneNumber;
    }

    public enum OtpType
    {
        PhoneNumber = 1,
        Email = 2
    }
}
