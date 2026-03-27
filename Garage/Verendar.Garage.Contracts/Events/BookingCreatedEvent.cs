using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingCreatedEvent : BaseEvent
{
    public override string EventType => "garage.booking.created.v1";

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public Guid GarageBranchId { get; set; }
    public Guid GarageProductId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
}
