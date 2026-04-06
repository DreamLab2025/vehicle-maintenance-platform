using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events
{
    public class OdometerUpdatedEvent : BaseEvent
    {
        public override string EventType => "vehicle.odometer.updated.v1";

        public Guid UserVehicleId { get; set; }

        public Guid UserId { get; set; }

        public int NewOdometerValue { get; set; }

        public DateOnly RecordedDate { get; set; }

        public int TotalEntryCount { get; set; }
    }
}
