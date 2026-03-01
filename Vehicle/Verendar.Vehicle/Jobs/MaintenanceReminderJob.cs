using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Application.Clients;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Jobs
{
    public class MaintenanceReminderJob(
        IUnitOfWork unitOfWork,
        IIdentityServiceClient identityClient,
        IPublishEndpoint publishEndpoint,
        ILogger<MaintenanceReminderJob> logger)
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("MaintenanceReminderJob: Finding Critical reminders (daily until replaced)");

            var reminders = await unitOfWork.MaintenanceReminders.GetByLevelWithDetailsAsync(
                ReminderLevel.Critical,
                includeAlreadyNotified: true,
                cancellationToken);

            var remindersList = reminders.ToList();
            if (remindersList.Count == 0)
            {
                logger.LogInformation("MaintenanceReminderJob: No Critical reminders");
                return;
            }

            var byUser = remindersList
                .Where(r => r.PartTracking?.UserVehicle != null && r.PartTracking.PartCategory != null)
                .GroupBy(r => r.PartTracking!.UserVehicle!.UserId);

            logger.LogInformation("MaintenanceReminderJob: Publishing reminder events for {UserCount} users ({ReminderCount} Critical reminders)",
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

                    var items = group.Select(r => r.ToEventItem()).ToList();

                    await publishEndpoint.Publish(new MaintenanceReminderEvent
                    {
                        UserId = userId,
                        TargetValue = email,
                        UserName = null,
                        Level = (int)ReminderLevel.Critical,
                        LevelName = nameof(ReminderLevel.Critical),
                        Items = items
                    }, cancellationToken);

                    logger.LogDebug("MaintenanceReminderJob: Published MaintenanceReminderEvent for user {UserId}, {Count} parts", userId, items.Count);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "MaintenanceReminderJob: Failed to publish for user {UserId}", userId);
                }
            }
        }
    }
}
