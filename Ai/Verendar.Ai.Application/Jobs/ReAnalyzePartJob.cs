namespace Verendar.Ai.Application.Jobs
{
    public class ReAnalyzePartJob(
        IVehicleMaintenanceAnalysisService analysisService,
        IVehicleServiceClient vehicleServiceClient,
        ILogger<ReAnalyzePartJob> logger)
    {
        private readonly IVehicleMaintenanceAnalysisService _analysisService = analysisService;
        private readonly IVehicleServiceClient _vehicleServiceClient = vehicleServiceClient;
        private readonly ILogger<ReAnalyzePartJob> _logger = logger;

        public async Task ExecuteAsync(
            Guid userVehicleId,
            Guid userId,
            string partCategorySlug,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "ReAnalyzePartJob: re-analyzing vehicle {VehicleId} part {Slug}",
                userVehicleId, partCategorySlug);

            var request = new VehicleQuestionnaireRequest
            {
                UserVehicleId = userVehicleId,
                PartCategorySlug = partCategorySlug,
                Answers = []
            };

            var analysisResult = await _analysisService.AnalyzeQuestionnaireAsync(userId, request, cancellationToken);

            if (!analysisResult.IsSuccess || analysisResult.Data == null || !analysisResult.Data.Recommendations.Any())
            {
                _logger.LogWarning(
                    "ReAnalyzePartJob: analysis failed for vehicle {VehicleId} part {Slug}: {Msg}",
                    userVehicleId, partCategorySlug, analysisResult.Message);
                return;
            }

            var rec = analysisResult.Data.Recommendations[0];

            var applyRequest = new VehicleServiceApplyTrackingRequest
            {
                PartCategorySlug = partCategorySlug,
                LastReplacementOdometer = rec.LastReplacementOdometer,
                LastReplacementDate = rec.LastReplacementDate,
                PredictedNextOdometer = rec.PredictedNextOdometer,
                PredictedNextDate = rec.PredictedNextDate,
                AiReasoning = rec.Reasoning,
                IsBaseline = false
            };

            var applyResult = await _vehicleServiceClient.ApplyTrackingInternalAsync(
                userVehicleId, userId, applyRequest, cancellationToken);

            if (!applyResult.IsSuccess)
            {
                _logger.LogWarning(
                    "ReAnalyzePartJob: apply-tracking failed for vehicle {VehicleId} part {Slug}: {Msg}",
                    userVehicleId, partCategorySlug, applyResult.Message);
                return;
            }

            _logger.LogInformation(
                "ReAnalyzePartJob: completed re-analysis for vehicle {VehicleId} part {Slug}",
                userVehicleId, partCategorySlug);
        }
    }
}
