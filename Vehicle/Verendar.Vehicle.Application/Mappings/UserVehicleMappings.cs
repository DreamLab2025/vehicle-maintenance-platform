using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Mappings
{
    public static class UserVehicleMappings
    {
        public static UserVehicle ToEntity(this UserVehicleRequest request, Guid userId)
        {
            return new UserVehicle
            {
                UserId = userId,
                VehicleVariantId = request.VehicleVariantId,
                LicensePlate = request.LicensePlate,
                VIN = request.VinNumber,
                PurchaseDate = request.PurchaseDate.HasValue ? DateOnly.FromDateTime(request.PurchaseDate.Value) : null,
                CurrentOdometer = request.CurrentOdometer,
                LastOdometerUpdate = DateOnly.FromDateTime(DateTime.UtcNow),
                AverageKmPerDay = null
            };
        }

        public static UserVehicleResponse ToResponse(this UserVehicle entity)
        {
            return new UserVehicleResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserVehicleVariant = entity.Variant!.ToUserVehicleVariantResponse(),
                LicensePlate = entity.LicensePlate,
                VinNumber = entity.VIN,
                PurchaseDate = entity.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdateAt = entity.LastOdometerUpdate?.ToDateTime(TimeOnly.MinValue),
                AverageKmPerDay = entity.AverageKmPerDay,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static UserVehicleDetailResponse ToDetailResponse(this UserVehicle entity, int totalMaintenanceActivities = 0, DateTime? lastMaintenanceDate = null)
        {
            var daysSincePurchase = entity.PurchaseDate.HasValue
                ? (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - entity.PurchaseDate.Value.DayNumber)
                : 0;

            return new UserVehicleDetailResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserVehicleVariant = entity.Variant!.ToUserVehicleVariantResponse(),
                LicensePlate = entity.LicensePlate,
                VinNumber = entity.VIN,
                PurchaseDate = entity.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdateAt = entity.LastOdometerUpdate?.ToDateTime(TimeOnly.MinValue),
                AverageKmPerDay = entity.AverageKmPerDay,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                TotalMaintenanceActivities = totalMaintenanceActivities,
                LastMaintenanceDate = lastMaintenanceDate,
                DaysSincePurchase = daysSincePurchase,
                TotalKmDriven = entity.CurrentOdometer,
                PartTrackings = entity.PartTrackings?.Select(pt => pt.ToSummary()).ToList() ?? new()
            };
        }

        public static VehiclePartTrackingSummary ToSummary(this VehiclePartTracking entity)
        {
            return new VehiclePartTrackingSummary
            {
                Id = entity.Id,
                PartCategoryId = entity.PartCategoryId,
                PartCategoryName = entity.PartCategory?.Name ?? string.Empty,
                PartCategoryCode = entity.PartCategory?.Code ?? string.Empty,
                InstanceIdentifier = entity.InstanceIdentifier,
                CurrentPartProductId = entity.CurrentPartProductId,
                CurrentPartProductName = entity.CurrentPartProduct?.Name,
                LastReplacementOdometer = entity.LastReplacementOdometer,
                LastReplacementDate = entity.LastReplacementDate,
                CustomKmInterval = entity.CustomKmInterval,
                CustomMonthsInterval = entity.CustomMonthsInterval,
                PredictedNextOdometer = entity.PredictedNextOdometer,
                PredictedNextDate = entity.PredictedNextDate,
                IsIgnored = entity.IsIgnored,
                UserConditionDescription = entity.UserConditionDescription,
                AiAnalysisResult = entity.AiAnalysisResult,
                Reminders = entity.Reminders?.Select(r => r.ToSummary()).ToList() ?? new()
            };
        }

        public static MaintenanceReminderSummary ToSummary(this MaintenanceReminder entity)
        {
            return new MaintenanceReminderSummary
            {
                Id = entity.Id,
                Level = entity.Level.ToString(),
                CurrentOdometer = entity.CurrentOdometer,
                TargetOdometer = entity.TargetOdometer,
                TargetDate = entity.TargetDate,
                PercentageRemaining = entity.PercentageRemaining,
                IsNotified = entity.IsNotified,
                NotifiedDate = entity.NotifiedDate,
                IsDismissed = entity.IsDismissed,
                DismissedDate = entity.DismissedDate
            };
        }

        public static void UpdateEntity(this UserVehicle entity, UserVehicleRequest request)
        {
            entity.VehicleVariantId = request.VehicleVariantId;
            entity.LicensePlate = request.LicensePlate;
            entity.VIN = request.VinNumber;
            entity.PurchaseDate = request.PurchaseDate.HasValue ? DateOnly.FromDateTime(request.PurchaseDate.Value) : null;
        }

        public static void UpdateOdometer(this UserVehicle entity, int newOdometer)
        {
            var oldOdometer = entity.CurrentOdometer;
            entity.CurrentOdometer = newOdometer;
            entity.LastOdometerUpdate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Calculate average km per day
            if (entity.PurchaseDate.HasValue)
            {
                var daysSincePurchase = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - entity.PurchaseDate.Value.DayNumber;
                if (daysSincePurchase > 0)
                {
                    entity.AverageKmPerDay = newOdometer / daysSincePurchase;
                }
            }
        }

        public static VehicleStreakResponse ToStreakResponse(this int streak, Guid userVehicleId)
        {
            return new VehicleStreakResponse
            {
                VehicleId = userVehicleId,
                CurrentStreak = streak,
                IsStreakActive = streak > 0,
                DaysToNextUnlock = streak > 0 ? 7 - (streak % 7) : 7
            };
        }
    }
}
