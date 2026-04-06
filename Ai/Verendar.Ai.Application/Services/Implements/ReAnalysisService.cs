using Hangfire;
using Verendar.Ai.Application.Jobs;

namespace Verendar.Ai.Application.Services.Implements
{
    public class ReAnalysisService(
        IVehicleServiceClient vehicleServiceClient,
        ILogger<ReAnalysisService> logger) : IReAnalysisService
    {
        private readonly IVehicleServiceClient _vehicleServiceClient = vehicleServiceClient;
        private readonly ILogger<ReAnalysisService> _logger = logger;

        public async Task QueueReAnalysisForBaselinePartsAsync(Guid userVehicleId, Guid userId)
        {
            ApiResponse<List<VehicleServiceBaselinePartItem>> partsResult;
            try
            {
                partsResult = await _vehicleServiceClient.GetBaselinePartsAsync(userVehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ReAnalysis: failed to get baseline parts for vehicle {VehicleId}", userVehicleId);
                return;
            }

            if (!partsResult.IsSuccess || partsResult.Data == null || partsResult.Data.Count == 0)
            {
                _logger.LogInformation("ReAnalysis: no baseline parts for vehicle {VehicleId}", userVehicleId);
                return;
            }

            foreach (var part in partsResult.Data)
            {
                BackgroundJob.Enqueue<ReAnalyzePartJob>(j =>
                    j.ExecuteAsync(userVehicleId, userId, part.PartCategorySlug, CancellationToken.None));

                _logger.LogInformation(
                    "ReAnalysis: enqueued job for vehicle {VehicleId} part {Slug}",
                    userVehicleId, part.PartCategorySlug);
            }
        }
    }
}
