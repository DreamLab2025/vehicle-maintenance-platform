using Verendar.Vehicle.Domain.Enums;

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
                VIN = request.VIN,
                PurchaseDate = request.PurchaseDate,
                CurrentOdometer = request.CurrentOdometer,
                LastOdometerUpdate = DateOnly.FromDateTime(DateTime.UtcNow),
                AverageKmPerDay = null
            };
        }

        public static UpdateOdometerResponse ToUpdateOdometerResponse(this UserVehicle entity)
        {
            return new UpdateOdometerResponse
            {
                UserVehicleId = entity.Id,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdate = entity.LastOdometerUpdate
            };
        }

        public static UserVehicleSummaryDto ToSummaryDto(this UserVehicle entity)
        {
            var variant = entity.Variant;
            var model = variant?.VehicleModel;
            var brand = model?.Brand;
            var type = brand?.VehicleType;

            return new UserVehicleSummaryDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                LicensePlate = entity.LicensePlate,
                VIN = entity.VIN,
                PurchaseDate = entity.PurchaseDate,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdate = entity.LastOdometerUpdate,
                AverageKmPerDay = entity.AverageKmPerDay,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Variant = new UserVehicleVariantSummaryDto
                {
                    Id = variant?.Id ?? Guid.Empty,
                    Color = variant?.Color ?? string.Empty,
                    HexCode = variant?.HexCode ?? string.Empty,
                    ImageUrl = variant?.ImageUrl ?? string.Empty,
                    ImageMediaFileId = variant?.ImageMediaFileId,
                    Model = new VehicleModelRefSummaryDto
                    {
                        Id = model?.Id ?? Guid.Empty,
                        Name = model?.Name ?? string.Empty,
                        Slug = model?.Slug ?? string.Empty,
                        Brand = new VehicleBrandRefSummaryDto
                        {
                            Id = brand?.Id ?? Guid.Empty,
                            Name = brand?.Name ?? string.Empty,
                            Slug = brand?.Slug ?? string.Empty,
                            Type = new VehicleTypeRefSummaryDto
                            {
                                Id = type?.Id ?? brand?.VehicleTypeId ?? Guid.Empty,
                                Name = type?.Name ?? string.Empty,
                                Slug = type?.Slug ?? string.Empty
                            }
                        }
                    }
                }
            };
        }

        public static UserVehicleResponse ToResponse(this UserVehicle entity)
        {
            return new UserVehicleResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserVehicleVariant = entity.Variant!.ToUserVariantResponse(),
                LicensePlate = entity.LicensePlate,
                VIN = entity.VIN,
                PurchaseDate = entity.PurchaseDate,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdate = entity.LastOdometerUpdate,
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
                UserVehicleVariant = entity.Variant!.ToUserVariantResponse(),
                LicensePlate = entity.LicensePlate,
                VIN = entity.VIN,
                PurchaseDate = entity.PurchaseDate,
                CurrentOdometer = entity.CurrentOdometer,
                LastOdometerUpdate = entity.LastOdometerUpdate,
                AverageKmPerDay = entity.AverageKmPerDay,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                TotalMaintenanceActivities = totalMaintenanceActivities,
                LastMaintenanceDate = lastMaintenanceDate,
                DaysSincePurchase = daysSincePurchase,
                TotalKmDriven = entity.CurrentOdometer
            };
        }

        public static PartSummary ToPartSummary(this PartTracking entity)
        {
            return new PartSummary
            {
                Id = entity.Id,
                PartCategoryId = entity.PartCategoryId,
                PartCategoryName = entity.PartCategory?.Name ?? string.Empty,
                PartCategorySlug = entity.PartCategory?.Slug ?? string.Empty,
                IconUrl = entity.PartCategory?.IconUrl,
                IsDeclared = entity.IsDeclared,
                Description = entity.PartCategory?.Description
            };
        }

        public static TrackingCycleSummary ToSummary(this TrackingCycle cycle, int? vehicleCurrentOdometer = null)
        {
            return new TrackingCycleSummary
            {
                Id = cycle.Id,
                Status = cycle.Status.ToString(),
                StartOdometer = cycle.StartOdometer,
                StartDate = cycle.StartDate,
                TargetOdometer = cycle.TargetOdometer,
                TargetDate = cycle.TargetDate,
                Reminders = cycle.Reminders
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.ToSummary(vehicleCurrentOdometer))
                    .ToList()
            };
        }

        public static PartTrackingSummary ToSummary(this PartTracking entity, int? vehicleCurrentOdometer = null)
        {
            var activeCycle = entity.Cycles?
                .Where(c => c.Status == CycleStatus.Active)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            return new PartTrackingSummary
            {
                Id = entity.Id,
                PartCategoryId = entity.PartCategoryId,
                PartCategoryName = entity.PartCategory?.Name ?? string.Empty,
                PartCategorySlug = entity.PartCategory?.Slug ?? string.Empty,
                InstanceIdentifier = entity.InstanceIdentifier,
                CurrentPartProductId = entity.CurrentPartProductId,
                CurrentPartProductName = entity.CurrentPartProduct?.Name,
                LastReplacementOdometer = entity.LastReplacementOdometer,
                LastReplacementDate = entity.LastReplacementDate,
                CustomKmInterval = entity.CustomKmInterval,
                CustomMonthsInterval = entity.CustomMonthsInterval,
                PredictedNextOdometer = entity.PredictedNextOdometer,
                PredictedNextDate = entity.PredictedNextDate,
                IsDeclared = entity.IsDeclared,
                ActiveCycle = activeCycle?.ToSummary(vehicleCurrentOdometer)
            };
        }

        public static ReminderSummary ToSummary(this MaintenanceReminder entity, int? vehicleCurrentOdometer = null)
        {
            var currentOdo = vehicleCurrentOdometer ?? entity.CurrentOdometer;
            return new ReminderSummary
            {
                Id = entity.Id,
                Level = entity.Level.ToString(),
                Status = entity.Status.ToString(),
                CurrentOdometer = currentOdo,
                TargetOdometer = entity.TargetOdometer,
                RemainingKm = entity.TargetOdometer - currentOdo,
                TargetDate = entity.TargetDate,
                PercentageRemaining = entity.PercentageRemaining,
                IsNotified = entity.IsNotified,
                NotifiedDate = entity.NotifiedDate,
                IsDismissed = entity.IsDismissed,
                DismissedDate = entity.DismissedDate,
            };
        }

        public static void UpdateFromRequest(this UserVehicle entity, UserVehicleRequest request)
        {
            entity.VehicleVariantId = request.VehicleVariantId;
            entity.LicensePlate = request.LicensePlate;
            entity.VIN = request.VIN;
            entity.PurchaseDate = request.PurchaseDate;
        }

        public static void UpdateOdometer(this UserVehicle entity, int newOdometer)
        {
            var oldOdometer = entity.CurrentOdometer;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (entity.LastOdometerUpdate.HasValue && newOdometer > oldOdometer)
            {
                var daysSinceLastUpdate = today.DayNumber - entity.LastOdometerUpdate.Value.DayNumber;
                if (daysSinceLastUpdate > 0)
                {
                    entity.AverageKmPerDay = (newOdometer - oldOdometer) / daysSinceLastUpdate;
                }
            }

            entity.CurrentOdometer = newOdometer;
            entity.LastOdometerUpdate = today;
        }

        public static StreakResponse ToStreakResponse(this int streak, Guid userVehicleId)
        {
            return new StreakResponse
            {
                VehicleId = userVehicleId,
                CurrentStreak = streak,
                IsStreakActive = streak > 0,
                DaysToNextUnlock = streak > 0 ? 7 - (streak % 7) : 7
            };
        }

        public static IsAllowedToCreateVehicleResponse ToIsAllowedToCreateVehicleResponse(this bool isAllowed, string message)
        {
            return new IsAllowedToCreateVehicleResponse
            {
                IsAllowed = isAllowed,
                Message = message
            };
        }

        public static PartTracking ToInitializePartTracking(this Guid userVehicleId, Guid partCategoryId)
        {
            return new PartTracking
            {
                UserVehicleId = userVehicleId,
                PartCategoryId = partCategoryId,
                IsDeclared = false,
            };
        }

        public static PartTracking ToPartTracking(this Guid userVehicleId, Guid partCategoryId, ApplyTrackingConfigRequest request)
        {
            return new PartTracking
            {
                UserVehicleId = userVehicleId,
                PartCategoryId = partCategoryId,
                IsDeclared = true,
                LastReplacementOdometer = request.LastReplacementOdometer,
                LastReplacementDate = request.LastReplacementDate,
                PredictedNextOdometer = request.PredictedNextOdometer,
                PredictedNextDate = request.PredictedNextDate,
            };
        }


        public static void ApplyTrackingConfig(this PartTracking entity, ApplyTrackingConfigRequest request)
        {
            entity.LastReplacementOdometer = request.LastReplacementOdometer;
            entity.LastReplacementDate = request.LastReplacementDate;
            entity.PredictedNextOdometer = request.PredictedNextOdometer;
            entity.PredictedNextDate = request.PredictedNextDate;
            entity.IsDeclared = true;
        }

        public static OdometerHistory ToOdometerHistory(this Guid userVehicleId, int odometerValue)
        {
            return new OdometerHistory
            {
                UserVehicleId = userVehicleId,
                OdometerValue = odometerValue,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Source = OdometerSource.ManualInput,
                KmOnRecordedDate = null
            };
        }

        public static OdometerHistory ToOdometerHistory(this Guid userVehicleId, int odometerValue, int previousOdometerValue)
        {
            return new OdometerHistory
            {
                UserVehicleId = userVehicleId,
                OdometerValue = odometerValue,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Source = OdometerSource.ManualInput,
                KmOnRecordedDate = odometerValue - previousOdometerValue
            };
        }

        public static OdometerHistory ToPhotoInputOdometerHistory(this Guid userVehicleId, int odometerValue, int previousOdometerValue, Guid? mediaFileId = null)
        {
            return new OdometerHistory
            {
                UserVehicleId = userVehicleId,
                OdometerValue = odometerValue,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Source = OdometerSource.PhotoInput,
                KmOnRecordedDate = odometerValue - previousOdometerValue,
                MediaFileId = mediaFileId
            };
        }

        public static ReminderDetailDto ToReminderDetailDto(this MaintenanceReminder entity, int? vehicleCurrentOdometer = null)
        {
            var currentOdo = vehicleCurrentOdometer ?? entity.CurrentOdometer;
            return new ReminderDetailDto
            {
                Id = entity.Id,
                TrackingCycleId = entity.TrackingCycleId,
                Level = entity.Level.ToString(),
                Status = entity.Status.ToString(),
                CurrentOdometer = currentOdo,
                TargetOdometer = entity.TargetOdometer,
                RemainingKm = entity.TargetOdometer - currentOdo,
                TargetDate = entity.TargetDate,
                PercentageRemaining = entity.PercentageRemaining,
                IsNotified = entity.IsNotified,
                NotifiedDate = entity.NotifiedDate,
                IsDismissed = entity.IsDismissed,
                DismissedDate = entity.DismissedDate,
                PartCategory = entity.TrackingCycle?.PartTracking?.PartCategory?.ToCategoryInfoDto() ?? new CategoryInfoDto()
            };
        }

        public static CategoryInfoDto ToCategoryInfoDto(this PartCategory entity)
        {
            return new CategoryInfoDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                IconUrl = entity.IconUrl,
                IconMediaFileId = entity.IconMediaFileId,
                IdentificationSigns = entity.IdentificationSigns,
                ConsequencesIfNotHandled = entity.ConsequencesIfNotHandled
            };
        }

        public static OdometerHistoryItemDto ToOdometerHistoryItemDto(this OdometerHistory entity)
        {
            return new OdometerHistoryItemDto
            {
                Id = entity.Id,
                UserVehicleId = entity.UserVehicleId,
                OdometerValue = entity.OdometerValue,
                RecordedDate = entity.RecordedDate,
                KmOnRecordedDate = entity.KmOnRecordedDate,
                Source = entity.Source.ToString()
            };
        }

        public static VehicleHealthScoreResponse ToHealthScoreResponse(this IEnumerable<PartTracking> trackings, Guid vehicleId)
        {
            var breakdown = trackings.Select(t => t.ToHealthItem()).ToList();

            decimal? score = breakdown.Count == 0
                ? null
                : breakdown.Average(p => (decimal)p.HealthScore);

            return new VehicleHealthScoreResponse
            {
                VehicleId = vehicleId,
                Score = score,
                TrackedPartCount = breakdown.Count,
                Breakdown = breakdown
            };
        }

        public static PartHealthItem ToHealthItem(this PartTracking tracking)
        {
            var activeCycle = tracking.Cycles
                .Where(c => c.Status == CycleStatus.Active)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            var activeReminder = activeCycle?.Reminders
                .Where(r => r.Status == ReminderStatus.Active)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            var score = (int)(activeReminder?.PercentageRemaining ?? 100);
            var status = score >= 50 ? "Healthy" : score > 0 ? "Warning" : "Overdue";

            return new PartHealthItem
            {
                PartTrackingId = tracking.Id,
                PartCategorySlug = tracking.PartCategory?.Slug ?? string.Empty,
                PartCategoryName = tracking.PartCategory?.Name ?? string.Empty,
                IconUrl = tracking.PartCategory?.IconUrl,
                HealthScore = score,
                Status = status
            };
        }
    }
}
