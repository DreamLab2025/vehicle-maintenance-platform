using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Clients;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Jobs;

public class MaintenanceReminderJob(
    IUnitOfWork unitOfWork,
    IIdentityServiceClient identityClient,
    IPublishEndpoint publishEndpoint,
    ILogger<MaintenanceReminderJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("MaintenanceReminderJob: Finding urgent reminders (daily until replaced)");

        var reminders = await unitOfWork.MaintenanceReminders.GetByLevelWithDetailsAsync(
            ReminderLevel.Urgent,
            includeAlreadyNotified: true,
            cancellationToken);

        var remindersList = reminders.ToList();
        if (remindersList.Count == 0)
        {
            logger.LogInformation("MaintenanceReminderJob: No urgent reminders");
            return;
        }

        var byUser = remindersList
            .Where(r => r.PartTracking?.UserVehicle != null && r.PartTracking.PartCategory != null)
            .GroupBy(r => r.PartTracking!.UserVehicle!.UserId);

        logger.LogInformation("MaintenanceReminderJob: Publishing reminder events for {UserCount} users ({ReminderCount} urgent reminders)",
            byUser.Count(), remindersList.Count);

        foreach (var group in byUser)
        {
            var userId = group.Key;
            try
            {
                var email = await identityClient.GetUserEmailByIdAsync(userId, cancellationToken);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("MaintenanceReminderJob: No email for user {UserId}, skipping", userId);
                    continue;
                }

                var items = group.Select(r =>
                {
                    var uv = r.PartTracking!.UserVehicle!;
                    var vehicleDisplay = uv.Variant?.VehicleModel != null
                        ? $"{uv.Variant.VehicleModel.Name}" + (string.IsNullOrEmpty(uv.LicensePlate) ? "" : $" - {uv.LicensePlate}")
                        : uv.LicensePlate;
                    return new MaintenanceReminderItemDto
                    {
                        PartCategoryName = r.PartTracking!.PartCategory!.Name,
                        UserVehicleId = r.PartTracking.UserVehicleId,
                        ReminderId = r.Id,
                        CurrentOdometer = r.CurrentOdometer,
                        TargetOdometer = r.TargetOdometer,
                        InitialOdometer = r.PartTracking.LastReplacementOdometer,
                        PercentageRemaining = r.PercentageRemaining,
                        VehicleDisplayName = vehicleDisplay
                    };
                }).ToList();

                await publishEndpoint.Publish(new MaintenanceReminderEvent
                {
                    UserId = userId,
                    TargetValue = email,
                    UserName = null,
                    Level = (int)ReminderLevel.Urgent,
                    LevelName = nameof(ReminderLevel.Urgent),
                    Items = items
                }, cancellationToken);

                logger.LogDebug("MaintenanceReminderJob: Published MaintenanceReminderEvent for user {UserId}, {Count} parts",
                    userId, items.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MaintenanceReminderJob: Failed to publish for user {UserId}", userId);
            }
        }
    }
}
