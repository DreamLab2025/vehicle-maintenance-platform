using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Vehicle.Contracts.Events;
using Verendar.Vehicle.Clients;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Jobs
{
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

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

                    var vehicles = await unitOfWork.UserVehicles.GetStaleOdometerVehiclesByUserAsync(userId, StaleOdometerDays, cancellationToken);
                    var vehicleDtos = vehicles.Select(v =>
                    {
                        var lastUpdate = v.LastOdometerUpdate;
                        var daysSince = lastUpdate.HasValue ? today.DayNumber - lastUpdate.Value.DayNumber : (int?)null;
                        var displayName = v.Variant?.VehicleModel != null
                            ? $"{v.Variant.VehicleModel.Name}" + (string.IsNullOrEmpty(v.LicensePlate) ? "" : $" - {v.LicensePlate}")
                            : v.LicensePlate ?? "Xe của bạn";
                        return new OdometerReminderVehicleDto
                        {
                            UserVehicleId = v.Id,
                            VehicleDisplayName = displayName,
                            LicensePlate = v.LicensePlate,
                            CurrentOdometer = v.CurrentOdometer,
                            LastOdometerUpdate = v.LastOdometerUpdate,
                            DaysSinceUpdate = daysSince ?? StaleOdometerDays
                        };
                    }).ToList();

                    await publishEndpoint.Publish(new OdometerReminderEvent
                    {
                        UserId = userId,
                        TargetValue = email,
                        UserName = null,
                        StaleOdometerDays = StaleOdometerDays,
                        Vehicles = vehicleDtos
                    }, cancellationToken);

                    logger.LogDebug("OdometerReminderJob: Published OdometerReminderEvent for user {UserId}, {VehicleCount} vehicles", userId, vehicleDtos.Count);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "OdometerReminderJob: Failed to publish for user {UserId}", userId);
                }
            }
        }
    }
}
