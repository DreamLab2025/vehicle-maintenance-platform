using Verendar.Common.Contracts;

namespace Verendar.Identity.Contract;

public class OtpRequestedEvent : BaseEvent
{
    public override string EventType => "OtpRequested";
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
