using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events;

public class OdometerReminderEvent : BaseEvent
{
    public override string EventType => "vehicle.odometer.reminder.v1";

    public Guid UserId { get; set; }

    public string? TargetValue { get; set; }
}
