using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Clients;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Jobs;

public class OdometerReminderJob(
    IUnitOfWork unitOfWork,
    IIdentityServiceClient identityClient,
    IPublishEndpoint publishEndpoint,
    ILogger<OdometerReminderJob> logger)
{
    private const int StaleOdometerDays = 3;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("OdometerReminderJob: Finding users with no ODO update in last {Days} days", StaleOdometerDays);

        var userIds = await unitOfWork.UserVehicles.GetDistinctUserIdsWithStaleOdometerAsync(StaleOdometerDays, cancellationToken);

        if (userIds.Count == 0)
        {
            logger.LogInformation("OdometerReminderJob: No users with stale odometer");
            return;
        }

        logger.LogInformation("OdometerReminderJob: Publishing reminder events for {Count} users", userIds.Count);

        foreach (var userId in userIds)
        {
            try
            {
                var email = await identityClient.GetUserEmailByIdAsync(userId, cancellationToken);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("OdometerReminderJob: No email for user {UserId}, skipping", userId);
                    continue;
                }

                await publishEndpoint.Publish(new OdometerReminderEvent
                {
                    UserId = userId,
                    TargetValue = email
                }, cancellationToken);

                logger.LogDebug("OdometerReminderJob: Published OdometerReminderEvent for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OdometerReminderJob: Failed to publish for user {UserId}", userId);
            }
        }
    }
}
