using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Application.Clients;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using ContractsLevel = Verendar.Vehicle.Contracts.Enums.ReminderLevel;
using DomainLevel = Verendar.Vehicle.Domain.Enums.ReminderLevel;

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
            logger.LogInformation("MaintenanceReminderJob: Finding {Level} reminders (daily until replaced)", DomainLevel.Critical);

            var reminders = await unitOfWork.MaintenanceReminders.GetByLevelWithDetailsAsync(
                DomainLevel.Critical,
                includeAlreadyNotified: true,
                cancellationToken);

            var remindersList = reminders.ToList();
            if (remindersList.Count == 0)
            {
                logger.LogInformation("MaintenanceReminderJob: No {Level} reminders", DomainLevel.Critical);
                return;
            }

            var byUser = remindersList
                .Where(r => r.TrackingCycle?.PartTracking?.UserVehicle != null && r.TrackingCycle.PartTracking.PartCategory != null)
                .GroupBy(r => r.TrackingCycle!.PartTracking!.UserVehicle!.UserId);

            logger.LogInformation("MaintenanceReminderJob: Publishing reminder events for {UserCount} users ({ReminderCount} {Level} reminders)",
                byUser.Count(), remindersList.Count, DomainLevel.Critical);

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
                        Level = ContractsLevel.Critical,
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
