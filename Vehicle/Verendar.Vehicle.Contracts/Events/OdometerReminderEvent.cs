using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events
{
    public class OdometerReminderEvent : BaseEvent
    {
        public override string EventType => "vehicle.odometer.reminder.v1";

        public Guid UserId { get; set; }

        public string? TargetValue { get; set; }

        public string? UserName { get; set; }

        public int StaleOdometerDays { get; set; }

        public List<OdometerReminderVehicleDto> Vehicles { get; set; } = [];
    }

    public class OdometerReminderVehicleDto
    {
        public Guid UserVehicleId { get; set; }
        public string VehicleDisplayName { get; set; } = string.Empty;
        public string? LicensePlate { get; set; }
        public int CurrentOdometer { get; set; }
        public DateOnly? LastOdometerUpdate { get; set; }
        public int DaysSinceUpdate { get; set; }
    }
}
