using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingCancelledEvent : BaseEvent
{
    public override string EventType => "garage.booking.cancelled.v1";

    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid GarageBranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CancelledAt { get; set; }
    public string? CustomerEmail { get; set; }
}
