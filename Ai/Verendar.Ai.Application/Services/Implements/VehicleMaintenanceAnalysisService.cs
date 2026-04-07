namespace Verendar.Ai.Application.Services.Implements
{
    public class VehicleMaintenanceAnalysisService(
        IAiPromptService promptService,
        IGenerativeAiServiceFactory generativeAiFactory,
        IVehicleServiceClient vehicleServiceClient,
        IPredictionComputationService predictionService,
        IConfidenceCalculationService confidenceService,
        ILogger<VehicleMaintenanceAnalysisService> logger) : IVehicleMaintenanceAnalysisService
    {
        private readonly IAiPromptService _promptService = promptService;
        private readonly IGenerativeAiServiceFactory _generativeAiFactory = generativeAiFactory;
        private readonly IVehicleServiceClient _vehicleServiceClient = vehicleServiceClient;
        private readonly IPredictionComputationService _predictionService = predictionService;
        private readonly IConfidenceCalculationService _confidenceService = confidenceService;
        private readonly ILogger<VehicleMaintenanceAnalysisService> _logger = logger;

        public async Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
            Guid userId,
            VehicleQuestionnaireRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Load prompt
            var promptResult = await _promptService.GetPromptAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, cancellationToken);
            if (!promptResult.IsSuccess || promptResult.Data == null)
            {
                _logger.LogError("AnalyzeQuestionnaire: failed to load prompt for AnalyzeMaintenanceQuestionnaire");
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể tải prompt AI");
            }

            // 2. [Parallel] GetUserVehicle + GetOdometerSummary (non-blocking)
            var vehicleTask = FetchUserVehicleAsync(request.UserVehicleId, cancellationToken);
            var summaryTask = FetchOdometerSummaryAsync(request.UserVehicleId, cancellationToken);

            await Task.WhenAll(vehicleTask, summaryTask);

            var vehicleResult = await vehicleTask;
            if (!vehicleResult.IsSuccess || vehicleResult.Data == null)
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(vehicleResult.Message ?? "Không thể lấy thông tin xe");

            var vehicle = vehicleResult.Data;
            var vehicleModelId = vehicle.UserVehicleVariant.VehicleModelId;

            var odometerSummary = await summaryTask; // null-safe; failure treated as no history

            // 3. GetDefaultSchedule — vehicleModelId comes from vehicle response, not request
            ApiResponse<VehicleServiceDefaultScheduleResponse> scheduleResponse;
            try
            {
                scheduleResponse = await _vehicleServiceClient.GetDefaultScheduleAsync(
                    vehicleModelId, request.PartCategorySlug, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle service GetDefaultSchedule failed for model {ModelId} part {Slug}", vehicleModelId, request.PartCategorySlug);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
            }

            if (!scheduleResponse.IsSuccess || scheduleResponse.Data == null)
            {
                _logger.LogWarning("AnalyzeQuestionnaire: default schedule failure model {ModelId} part {Slug}: {Msg}", vehicleModelId, request.PartCategorySlug, scheduleResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(scheduleResponse.Message ?? "Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
            }

            var vehicleInfo = vehicle.ToVehicleInfoDto();
            var schedule = scheduleResponse.Data.ToDefaultScheduleDto(request.PartCategorySlug);

            // 4. Detect phase
            var entryCount = odometerSummary?.EntryCount ?? 0;
            var analysisPhase = entryCount >= 2 ? "personalized" : "baseline";

            // 5. Compute prediction base
            var predictionBase = _predictionService.ComputeBase(vehicleInfo, schedule, odometerSummary);

            // 6. Build prompt and call Gemini
            var prompt = VehicleMaintenanceAnalysisPrompt.ApplyTemplate(
                promptResult.Data.Content, vehicleInfo, schedule, request.Answers, predictionBase, odometerSummary);

            var aiService = _generativeAiFactory.Create((AiProvider)promptResult.Data.Provider);

            ApiResponse<GenerativeAiResponse> aiResponse;
            try
            {
                aiResponse = await aiService.GenerateContentAsync(
                    prompt, AiOperation.AnalyzeMaintenanceQuestionnaire, userId, temperature: 0.5m);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateContentAsync failed for user {UserId}", userId);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể phân tích dữ liệu");
            }

            if (!aiResponse.IsSuccess || aiResponse.Data == null)
            {
                _logger.LogWarning("AnalyzeQuestionnaire: AI service returned failure: {Message}", aiResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(aiResponse.Message ?? "Không thể phân tích dữ liệu");
            }

            if (string.IsNullOrWhiteSpace(aiResponse.Data.Content))
            {
                _logger.LogWarning("AnalyzeQuestionnaire: AI response content is empty. Model: {Model}", aiResponse.Data.Model);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("AI không trả về nội dung hợp lệ");
            }

            // 7. Parse Gemini output
            GeminiVehicleAnalysisResult? analysisResult;
            try
            {
                analysisResult = JsonSerializer.Deserialize<GeminiVehicleAnalysisResult>(
                    aiResponse.Data.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI response JSON: {Content}", aiResponse.Data.Content);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse($"Không thể phân tích kết quả từ AI: {ex.Message}");
            }

            if (analysisResult == null || !analysisResult.Recommendations.Any())
            {
                _logger.LogWarning("AI returned no recommendations");
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("AI không thể đưa ra khuyến nghị");
            }

            if (analysisResult.Recommendations.Count != 1)
            {
                _logger.LogWarning("AI returned {Count} recommendations, expected 1. Part: {Slug}", analysisResult.Recommendations.Count, schedule.PartCategorySlug);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse($"AI trả về {analysisResult.Recommendations.Count} khuyến nghị thay vì 1. Vui lòng thử lại.");
            }

            var geminiRec = analysisResult.Recommendations[0];

            // 8. Apply adjustment factor (clamp 0.7–1.3; default 1.0 if AI skips it)
            var adjustmentFactor = Math.Clamp(
                geminiRec.UsageAdjustmentFactor is > 0 ? geminiRec.UsageAdjustmentFactor : 1.0,
                0.7, 1.3);

            var finalOdo = vehicleInfo.CurrentOdometer + (int)(predictionBase.KmIntervalUsed * adjustmentFactor);
            var finalDate = predictionBase.ExpectedNextDate;

            // 9. Compute range dates proportionally
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var expectedDays = predictionBase.ExpectedNextDate.DayNumber - today.DayNumber;
            var odoDiff = predictionBase.ExpectedNextOdometer - vehicleInfo.CurrentOdometer;

            DateOnly? earliestDate = null;
            DateOnly? latestDate = null;

            if (odoDiff > 0 && expectedDays > 0)
            {
                var earliestDays = (int)(expectedDays * (double)(predictionBase.EarliestNextOdometer - vehicleInfo.CurrentOdometer) / odoDiff);
                var latestDays = (int)(expectedDays * (double)(predictionBase.LatestNextOdometer - vehicleInfo.CurrentOdometer) / odoDiff);
                earliestDate = today.AddDays(Math.Max(0, earliestDays));
                latestDate = today.AddDays(Math.Max(0, latestDays));
            }

            // 10. Confidence score
            var confidence = _confidenceService.Calculate(new ConfidenceInput
            {
                OdometerEntryCount = entryCount,
                AiAdjustmentFactor = adjustmentFactor,
                ManufacturerKmInterval = schedule.KmInterval,
                FinalPredictedOdo = finalOdo,
                CurrentOdo = vehicleInfo.CurrentOdometer
            });

            // 11. Build rangeNarrowsWhen hints
            var rangeNarrowsWhen = BuildRangeNarrowsWhen(entryCount);

            // 12. Build response
            var lastReplacementDate = ParseDateOnly(geminiRec.LastServiceDate);
            var allWarnings = geminiRec.Warnings ?? [];

            var response = new VehicleQuestionnaireResponse
            {
                Recommendations =
                [
                    new PartTrackingRecommendation
                    {
                        PartCategorySlug = geminiRec.PartCategorySlug,
                        LastReplacementOdometer = geminiRec.LastServiceOdometer,
                        LastReplacementDate = lastReplacementDate,
                        PredictedNextOdometer = finalOdo,
                        PredictedNextDate = finalDate,
                        EarliestNextOdometer = predictionBase.EarliestNextOdometer,
                        LatestNextOdometer = predictionBase.LatestNextOdometer,
                        EarliestNextDate = earliestDate,
                        LatestNextDate = latestDate,
                        ConfidenceTier = confidence.Tier,
                        AnalysisPhase = analysisPhase,
                        RangeNarrowsWhen = rangeNarrowsWhen,
                        Reasoning = geminiRec.Reasoning,
                        NeedsImmediateAttention = geminiRec.NeedsImmediateAttention
                    }
                ],
                Warnings = allWarnings,
                Metadata = new AiAnalysisMetadata
                {
                    Model = aiResponse.Data.Model,
                    TotalTokens = aiResponse.Data.TotalTokens,
                    TotalCost = aiResponse.Data.TotalCost,
                    ResponseTimeMs = aiResponse.Data.ResponseTimeMs
                }
            };

            return ApiResponse<VehicleQuestionnaireResponse>.SuccessResponse(response);
        }

        private async Task<ApiResponse<VehicleServiceUserVehicleResponse>> FetchUserVehicleAsync(
            Guid userVehicleId, CancellationToken ct)
        {
            try
            {
                return await _vehicleServiceClient.GetUserVehicleByIdAsync(userVehicleId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle service GetUserVehicleById failed for {UserVehicleId}", userVehicleId);
                return ApiResponse<VehicleServiceUserVehicleResponse>.FailureResponse("Không thể lấy thông tin xe từ Vehicle Service");
            }
        }

        private async Task<VehicleServiceOdometerSummaryResponse?> FetchOdometerSummaryAsync(
            Guid userVehicleId, CancellationToken ct)
        {
            try
            {
                var result = await _vehicleServiceClient.GetOdometerSummaryAsync(userVehicleId, ct);
                return result.IsSuccess ? result.Data : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetOdometerSummary non-blocking failure for {UserVehicleId} — continuing as State 1", userVehicleId);
                return null;
            }
        }

        private static List<string> BuildRangeNarrowsWhen(int entryCount) => entryCount switch
        {
            0 => ["Cập nhật odometer ít nhất 2 lần để nhận dự đoán cá nhân hóa"],
            1 => ["Cập nhật odometer thêm 1 lần nữa để nhận dự đoán cá nhân hóa"],
            < 5 => ["Cập nhật odometer thêm vài lần để thu hẹp khoảng dự đoán"],
            _ => []
        };

        private static DateOnly? ParseDateOnly(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return DateOnly.TryParse(value, out var d) ? d : null;
        }
    }
}
