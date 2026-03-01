using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class MaintenanceReminderEventMappings
    {
        public static MaintenanceReminderItemDto ToEventItem(this MaintenanceReminder r)
        {
            var pt = r.PartTracking;
            var uv = pt?.UserVehicle;
            var vehicleDisplay = uv?.Variant?.VehicleModel != null
                ? $"{uv.Variant.VehicleModel.Name}" + (string.IsNullOrEmpty(uv.LicensePlate) ? "" : $" - {uv.LicensePlate}")
                : uv?.LicensePlate;

            DateTime? estimatedNextDate = pt?.PredictedNextDate.HasValue == true
                ? pt.PredictedNextDate!.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            return new MaintenanceReminderItemDto
            {
                PartCategoryName = pt?.PartCategory?.Name ?? string.Empty,
                Description = pt?.PartCategory?.Description,
                UserVehicleId = pt?.UserVehicleId ?? Guid.Empty,
                ReminderId = r.Id,
                CurrentOdometer = r.CurrentOdometer,
                TargetOdometer = r.TargetOdometer,
                InitialOdometer = pt?.LastReplacementOdometer,
                PercentageRemaining = r.PercentageRemaining,
                VehicleDisplayName = vehicleDisplay,
                EstimatedNextReplacementDate = estimatedNextDate
            };
        }
    }
}
