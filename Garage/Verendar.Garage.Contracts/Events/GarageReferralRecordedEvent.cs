using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class GarageReferralRecordedEvent : BaseEvent
{
    public override string EventType => "garage.referral.recorded.v1";

    public Guid GarageId { get; set; }
    public Guid GarageOwnerId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public Guid ReferredUserId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public DateTime ReferredAt { get; set; }
}
