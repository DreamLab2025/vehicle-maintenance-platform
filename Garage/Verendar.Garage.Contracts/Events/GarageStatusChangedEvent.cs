using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class GarageStatusChangedEvent : BaseEvent
{
    public override string EventType => "garage.status.changed.v1";

    public Guid GarageId { get; set; }
    public Guid OwnerId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}
