using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingCompletedEvent : BaseEvent
{
    public override string EventType => "garage.booking.completed.v1";

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public Guid GarageBranchId { get; set; }
    public Guid GarageProductId { get; set; }
    public int? CurrentOdometer { get; set; }
    public DateTime CompletedAt { get; set; }
}
