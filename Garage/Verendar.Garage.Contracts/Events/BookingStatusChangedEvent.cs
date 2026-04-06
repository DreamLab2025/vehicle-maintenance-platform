using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingStatusChangedEvent : BaseEvent
{
    public override string EventType => "garage.booking.status.changed.v1";

    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid GarageId { get; set; }
    public Guid GarageBranchId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
