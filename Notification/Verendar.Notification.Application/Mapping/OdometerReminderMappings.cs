using Verendar.Notification.Application.Constants;
using Verendar.Notification.Application.Dtos.Email;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Notification.Application.Mapping
{
    public static class OdometerReminderMappings
    {
        public static string OdometerReminderTitle => NotificationConstants.Titles.OdometerReminder;

        public static OdometerReminderEmailModel ToEmailModel(this OdometerReminderEvent message, int staleOdometerDays) => new()
        {
            UserName = message.UserName ?? string.Empty,
            UserEmail = message.TargetValue,
            Title = OdometerReminderTitle,
            StaleOdometerDays = staleOdometerDays,
            Vehicles = (message.Vehicles ?? []).Select(v => new OdometerReminderVehicleEmailDto
            {
                VehicleDisplayName = v.VehicleDisplayName ?? string.Empty,
                LicensePlate = v.LicensePlate,
                CurrentOdometer = v.CurrentOdometer,
                LastOdometerUpdateFormatted = v.LastOdometerUpdate?.ToString(NotificationConstants.DateFormats.DateOnly),
                DaysSinceUpdate = v.DaysSinceUpdate
            }).ToList()
        };
    }
}
