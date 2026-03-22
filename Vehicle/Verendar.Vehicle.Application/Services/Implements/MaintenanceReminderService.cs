using MassTransit;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Application.Clients;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Enums;
using ContractsLevel = Verendar.Vehicle.Contracts.Enums.ReminderLevel;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class MaintenanceReminderService(
        IUnitOfWork unitOfWork,
        IIdentityServiceClient identityClient,
        IPublishEndpoint publishEndpoint,
        ILogger<MaintenanceReminderService> logger) : IMaintenanceReminderService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IIdentityServiceClient _identityClient = identityClient;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        private readonly ILogger<MaintenanceReminderService> _logger = logger;

        public async Task<ApiResponse<List<ReminderDetailDto>>> GetRemindersAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetReminders: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<ReminderDetailDto>>.NotFoundResponse("Không tìm thấy xe");
            }

            var activeCycles = await _unitOfWork.TrackingCycles.GetActiveCyclesByVehicleIdAsync(userVehicleId);

            var dtos = activeCycles
                .Select(c => c.Reminders.FirstOrDefault(r => r.Status == ReminderStatus.Active))
                .Where(r => r != null)
                .Cast<MaintenanceReminder>()
                .Select(r => r.ToReminderDetailDto(vehicle.CurrentOdometer))
                .ToList();

            return ApiResponse<List<ReminderDetailDto>>.SuccessResponse(
                dtos,
                "Lấy danh sách nhắc bảo trì thành công");
        }

        public async Task SyncRemindersAsync(Guid vehicleId, int currentOdometer, Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var kmPerDay = await GetKmPerDayFromLast3MonthsAsync(vehicleId);

            if (kmPerDay is null)
            {
                var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(v => v.Id == vehicleId);
                if (vehicle?.AverageKmPerDay is > 0)
                    kmPerDay = vehicle.AverageKmPerDay.Value;
            }

            var activeCycles = await _unitOfWork.TrackingCycles.GetActiveCyclesByVehicleIdAsync(vehicleId);

            foreach (var cycle in activeCycles)
            {
                var tracking = cycle.PartTracking;
                if (!tracking.IsDeclared) continue;
                if (tracking.PredictedNextOdometer == null && tracking.PredictedNextDate == null) continue;

                var (percentageRemaining, targetDate) = ComputeReminderData(tracking, currentOdometer, today, kmPerDay);
                if (percentageRemaining is null) continue;

                var targetOdometer = tracking.PredictedNextOdometer ?? currentOdometer;
                var level = GetLevelFromPercentage(percentageRemaining.Value);

                if (currentOdometer >= targetOdometer || (targetDate.HasValue && today >= targetDate.Value))
                    level = ReminderLevel.Critical;

                var activeReminder = cycle.Reminders.FirstOrDefault(r => r.Status == ReminderStatus.Active);

                if (activeReminder == null)
                {
                    // Lazy: tạo reminder đầu tiên cho cycle này
                    var reminder = new MaintenanceReminder
                    {
                        TrackingCycleId = cycle.Id,
                        CurrentOdometer = currentOdometer,
                        TargetOdometer = targetOdometer,
                        TargetDate = targetDate,
                        Level = level,
                        PercentageRemaining = percentageRemaining.Value,
                        Status = ReminderStatus.Active,
                    };
                    await _unitOfWork.MaintenanceReminders.AddAsync(reminder);
                }
                else if (activeReminder.Level != level ||
                         activeReminder.TargetOdometer != targetOdometer ||
                         activeReminder.TargetDate != targetDate)
                {
                    // Level thay đổi → Passed rồi tạo Active mới
                    activeReminder.Status = ReminderStatus.Passed;

                    var reminder = new MaintenanceReminder
                    {
                        TrackingCycleId = cycle.Id,
                        CurrentOdometer = currentOdometer,
                        TargetOdometer = targetOdometer,
                        TargetDate = targetDate,
                        Level = level,
                        PercentageRemaining = percentageRemaining.Value,
                        Status = ReminderStatus.Active,
                    };
                    await _unitOfWork.MaintenanceReminders.AddAsync(reminder);
                }
                else
                {
                    // Cùng level → cập nhật vị trí
                    activeReminder.CurrentOdometer = currentOdometer;
                    activeReminder.PercentageRemaining = percentageRemaining.Value;
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PublishMaintenanceReminderIfNeededAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var reminders = (await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdWithDetailsAsync(vehicleId, cancellationToken)).ToList();

                if (reminders.Count == 0)
                    return;

                var byLevel = reminders
                    .Where(r => r.TrackingCycle?.PartTracking?.UserVehicle != null && r.TrackingCycle.PartTracking.PartCategory != null)
                    .GroupBy(r => r.Level)
                    .OrderByDescending(g => g.Key);

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                foreach (var levelGroup in byLevel)
                {
                    var level = levelGroup.Key;

                    if (level != ReminderLevel.Critical)
                        continue;

                    var alreadyNotifiedToday = levelGroup.All(r => r.IsNotified && r.NotifiedDate == today);
                    if (alreadyNotifiedToday)
                        continue;

                    try
                    {
                        var email = await _identityClient.GetUserEmailByIdAsync(userId, cancellationToken);
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            _logger.LogWarning("No email for user {UserId}, skipping notification", userId);
                            continue;
                        }

                        var items = levelGroup.Select(r => r.ToEventItem()).ToList();

                        await _publishEndpoint.Publish(new MaintenanceReminderEvent
                        {
                            UserId = userId,
                            TargetValue = email,
                            UserName = null,
                            Level = (ContractsLevel)(int)level,
                            Items = items
                        }, cancellationToken);

                        foreach (var reminder in levelGroup)
                        {
                            reminder.IsNotified = true;
                            reminder.NotifiedDate = today;
                            await _unitOfWork.MaintenanceReminders.UpdateAsync(reminder.Id, reminder);
                        }
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish MaintenanceReminderEvent for user {UserId}, vehicle {VehicleId}", userId, vehicleId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PublishMaintenanceReminderIfNeededAsync for vehicle {VehicleId}", vehicleId);
            }
        }

        private async Task<decimal?> GetKmPerDayFromLast3MonthsAsync(Guid vehicleId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var fromDate = today.AddMonths(-3);

            var history = await _unitOfWork.OdometerHistories.GetRecordedOnOrAfterOrderedAsync(vehicleId, fromDate);

            if (history.Count < 2)
                return null;

            var first = history[0];
            var last = history[^1];
            var totalKm = last.OdometerValue - first.OdometerValue;
            var totalDays = last.RecordedDate.DayNumber - first.RecordedDate.DayNumber;

            if (totalDays <= 0 || totalKm < 0)
                return null;

            return Math.Round((decimal)totalKm / totalDays, 2);
        }

        private static ReminderLevel GetLevelFromPercentage(decimal percentageRemaining)
        {
            if (percentageRemaining > 40) return ReminderLevel.Normal;
            if (percentageRemaining > 25) return ReminderLevel.Low;
            if (percentageRemaining > 15) return ReminderLevel.Medium;
            if (percentageRemaining > 5) return ReminderLevel.High;
            return ReminderLevel.Critical;
        }

        private static (decimal? PercentageRemaining, DateOnly? TargetDate) ComputeReminderData(
            PartTracking tracking,
            int currentOdometer,
            DateOnly today,
            decimal? kmPerDay)
        {
            decimal? percentageKm = null;
            decimal? percentageDate = null;
            DateOnly? targetDate = tracking.PredictedNextDate;

            if (tracking.PredictedNextOdometer.HasValue)
            {
                int intervalKm = tracking.LastReplacementOdometer.HasValue
                    ? tracking.PredictedNextOdometer.Value - tracking.LastReplacementOdometer.Value
                    : (tracking.CustomKmInterval ?? 1);
                if (intervalKm <= 0) intervalKm = 1;
                var remainingKm = tracking.PredictedNextOdometer.Value - currentOdometer;
                percentageKm = Math.Clamp(remainingKm * 100m / intervalKm, 0, 100);

                if (!tracking.PredictedNextDate.HasValue && kmPerDay.HasValue && kmPerDay > 0 && remainingKm > 0)
                {
                    var estimatedDaysRemaining = remainingKm / kmPerDay.Value;
                    var estimatedTargetDate = today.AddDays((int)Math.Ceiling(estimatedDaysRemaining));
                    targetDate = estimatedTargetDate;

                    var intervalDays = intervalKm / kmPerDay.Value;
                    if (intervalDays <= 0) intervalDays = 30;
                    percentageDate = Math.Clamp(estimatedDaysRemaining * 100m / intervalDays, 0, 100);
                }
            }

            if (tracking.PredictedNextDate.HasValue)
            {
                int intervalDays = tracking.LastReplacementDate.HasValue
                    ? tracking.PredictedNextDate.Value.DayNumber - tracking.LastReplacementDate.Value.DayNumber
                    : (tracking.CustomMonthsInterval ?? 1) * 30;
                if (intervalDays <= 0) intervalDays = 30;
                var remainingDays = tracking.PredictedNextDate.Value.DayNumber - today.DayNumber;
                percentageDate = Math.Clamp(remainingDays * 100m / intervalDays, 0, 100);
            }

            decimal? percentage = percentageKm.HasValue && percentageDate.HasValue
                ? Math.Min(percentageKm.Value, percentageDate.Value)
                : (percentageKm ?? percentageDate);

            return (percentage, targetDate);
        }
    }
}
