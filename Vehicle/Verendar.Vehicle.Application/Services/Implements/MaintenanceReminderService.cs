using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Application.Clients;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

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

        public async Task PublishMaintenanceReminderIfNeededAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var reminders = (await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdWithDetailsAsync(vehicleId, cancellationToken)).ToList();

                if (reminders.Count == 0)
                {
                    _logger.LogDebug("No reminders found for vehicle {VehicleId}", vehicleId);
                    return;
                }

                var byLevel = reminders
                    .Where(r => r.PartTracking?.UserVehicle != null && r.PartTracking.PartCategory != null)
                    .GroupBy(r => r.Level)
                    .OrderByDescending(g => g.Key);

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                foreach (var levelGroup in byLevel)
                {
                    var level = levelGroup.Key;

                    if (level != ReminderLevel.Critical)
                    {
                        _logger.LogDebug("Skipping level {Level} for vehicle {VehicleId} - will be handled by background job", level, vehicleId);
                        continue;
                    }

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

                        var items = levelGroup.Select(r => r.ToEventItem()).ToList();

                        await _publishEndpoint.Publish(new MaintenanceReminderEvent
                        {
                            UserId = userId,
                            TargetValue = email,
                            UserName = null,
                            Level = (int)level,
                            LevelName = level.ToString(),
                            Items = items
                        }, cancellationToken);

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
