namespace Verendar.Ai.Application.Services.Implements
{
    public class VehicleMaintenanceAnalysisService(
        IGenerativeAiService aiService,
        IVehicleServiceClient vehicleServiceClient,
        ILogger<VehicleMaintenanceAnalysisService> logger) : IVehicleMaintenanceAnalysisService
    {
        private readonly IGenerativeAiService _aiService = aiService;
        private readonly IVehicleServiceClient _vehicleServiceClient = vehicleServiceClient;
        private readonly ILogger<VehicleMaintenanceAnalysisService> _logger = logger;

        public async Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
            Guid userId,
            VehicleQuestionnaireRequest request,
            CancellationToken cancellationToken = default)
        {
            ApiResponse<VehicleServiceUserVehicleResponse> vehicleResponse;
            try
            {
                vehicleResponse = await _vehicleServiceClient.GetUserVehicleByIdAsync(request.UserVehicleId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle service GetUserVehicleById failed for {UserVehicleId}", request.UserVehicleId);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể lấy thông tin xe từ Vehicle Service");
            }

            if (!vehicleResponse.IsSuccess || vehicleResponse.Data == null)
            {
                _logger.LogWarning("AnalyzeQuestionnaire: vehicle service returned failure for {UserVehicleId}: {Message}", request.UserVehicleId, vehicleResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    vehicleResponse.Message ?? "Không thể lấy thông tin xe từ Vehicle Service");
            }

            var vehicle = vehicleResponse.Data;
            var vehicleInfo = vehicle.ToVehicleInfoDto();

            ApiResponse<VehicleServiceDefaultScheduleResponse> scheduleResponse;
            try
            {
                scheduleResponse = await _vehicleServiceClient.GetDefaultScheduleAsync(
                    request.VehicleModelId,
                    request.PartCategorySlug,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle service GetDefaultSchedule failed for model {VehicleModelId} part {PartCategorySlug}", request.VehicleModelId, request.PartCategorySlug);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
            }

            if (!scheduleResponse.IsSuccess || scheduleResponse.Data == null)
            {
                _logger.LogWarning("AnalyzeQuestionnaire: default schedule failure model {VehicleModelId} part {PartCategorySlug}: {Message}", request.VehicleModelId, request.PartCategorySlug, scheduleResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    scheduleResponse.Message ?? "Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
            }

            var schedule = scheduleResponse.Data;
            var defaultSchedule = schedule.ToDefaultScheduleDto(request.PartCategorySlug);

            var prompt = PromptGenerator.CreateVehicleMaintenancePrompt(vehicleInfo, defaultSchedule, request.Answers);

            ApiResponse<GenerativeAiResponse> aiResponse;
            try
            {
                aiResponse = await _aiService.GenerateContentAsync(
                    prompt,
                    AiOperation.GenerateText,
                    userId,
                    temperature: 0.5m
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateContentAsync failed for user {UserId}", userId);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse("Không thể phân tích dữ liệu");
            }

            if (!aiResponse.IsSuccess || aiResponse.Data == null)
            {
                _logger.LogWarning("AnalyzeQuestionnaire: AI service returned failure: {Message}", aiResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    aiResponse.Message ?? "Không thể phân tích dữ liệu");
            }

            var content = aiResponse.Data.Content;

            GeminiVehicleAnalysisResult? analysisResult;
            try
            {
                analysisResult = JsonSerializer.Deserialize<GeminiVehicleAnalysisResult>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI response JSON: {Content}", content);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    $"Không thể phân tích kết quả từ AI: {ex.Message}");
            }

            if (analysisResult == null || !analysisResult.Recommendations.Any())
            {
                _logger.LogWarning("AI returned no recommendations");
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "AI không thể đưa ra khuyến nghị");
            }

            if (analysisResult.Recommendations.Count != 1)
            {
                _logger.LogWarning(
                    "AI returned {Count} recommendations, expected 1. Requested part: {PartCode}",
                    analysisResult.Recommendations.Count,
                    defaultSchedule.PartCategorySlug);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    $"AI trả về {analysisResult.Recommendations.Count} khuyến nghị thay vì 1. Vui lòng thử lại.");
            }

            var response = analysisResult.ToResponse(
                aiResponse.Data
            );

            return ApiResponse<VehicleQuestionnaireResponse>.SuccessResponse(response);
        }
    }
}
