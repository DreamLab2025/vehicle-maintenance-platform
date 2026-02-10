using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Clients;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Services
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

        public async Task PublishMaintenanceReminderIfNeededAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all reminders for this vehicle
                var reminders = await _unitOfWork.MaintenanceReminders.AsQueryable()
                    .Include(r => r.PartTracking)
                        .ThenInclude(pt => pt.PartCategory)
                    .Include(r => r.PartTracking)
                        .ThenInclude(pt => pt.UserVehicle)
                            .ThenInclude(uv => uv.Variant)
                                .ThenInclude(v => v.VehicleModel)
                    .Where(r => r.PartTracking!.UserVehicleId == vehicleId)
                    .ToListAsync(cancellationToken);

                if (reminders.Count == 0)
                {
                    _logger.LogDebug("No reminders found for vehicle {VehicleId}", vehicleId);
                    return;
                }

                // Group by level and check if we need to send notification
                var byLevel = reminders
                    .Where(r => r.PartTracking?.UserVehicle != null && r.PartTracking.PartCategory != null)
                    .GroupBy(r => r.Level)
                    .OrderByDescending(g => g.Key);

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                foreach (var levelGroup in byLevel)
                {
                    var level = levelGroup.Key;

                    // Only send notification for Critical level immediately
                    // Other levels will be sent by background job
                    if (level != ReminderLevel.Critical)
                    {
                        _logger.LogDebug("Skipping level {Level} for vehicle {VehicleId} - will be handled by background job", level, vehicleId);
                        continue;
                    }

                    // Check if already notified today (send only once per day for Critical level)
                    var alreadyNotifiedToday = levelGroup.All(r => r.IsNotified && r.NotifiedDate == today);
                    if (alreadyNotifiedToday)
                    {
                        _logger.LogDebug("Critical reminders for vehicle {VehicleId} already notified today, skipping", vehicleId);
                        continue;
                    }

                    try
                    {
                        var email = await _identityClient.GetUserEmailByIdAsync(userId, cancellationToken);
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            _logger.LogWarning("No email for user {UserId}, skipping notification", userId);
                            continue;
                        }

                        var items = levelGroup.Select(r =>
                        {
                            var uv = r.PartTracking!.UserVehicle!;
                            var vehicleDisplay = uv.Variant?.VehicleModel != null
                                ? $"{uv.Variant.VehicleModel.Name}" + (string.IsNullOrEmpty(uv.LicensePlate) ? "" : $" - {uv.LicensePlate}")
                                : uv.LicensePlate;

                            // Convert DateOnly? to DateTime?
                            DateTime? estimatedNextDate = r.PartTracking.PredictedNextDate.HasValue
                                ? r.PartTracking.PredictedNextDate.Value.ToDateTime(TimeOnly.MinValue)
                                : null;

                            return new MaintenanceReminderItemDto
                            {
                                PartCategoryName = r.PartTracking!.PartCategory!.Name,
                                Description = r.PartTracking.PartCategory.Description,
                                UserVehicleId = r.PartTracking.UserVehicleId,
                                ReminderId = r.Id,
                                CurrentOdometer = r.CurrentOdometer,
                                TargetOdometer = r.TargetOdometer,
                                InitialOdometer = r.PartTracking.LastReplacementOdometer,
                                PercentageRemaining = r.PercentageRemaining,
                                VehicleDisplayName = vehicleDisplay,
                                EstimatedNextReplacementDate = estimatedNextDate
                            };
                        }).ToList();

                        await _publishEndpoint.Publish(new MaintenanceReminderEvent
                        {
                            UserId = userId,
                            TargetValue = email,
                            UserName = null,
                            Level = (int)level,
                            LevelName = level.ToString(),
                            Items = items
                        }, cancellationToken);

                        // Mark all reminders in this level as notified today
                        foreach (var reminder in levelGroup)
                        {
                            reminder.IsNotified = true;
                            reminder.NotifiedDate = today;
                            await _unitOfWork.MaintenanceReminders.UpdateAsync(reminder.Id, reminder);
                        }
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Published MaintenanceReminderEvent for user {UserId}, vehicle {VehicleId}, level {Level}, {Count} parts, marked as notified today",
                            userId, vehicleId, level, items.Count);
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
    }
}
