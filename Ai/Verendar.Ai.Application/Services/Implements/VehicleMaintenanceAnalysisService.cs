using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Helpers;
using Verendar.Ai.Application.Mappings;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Services.Implements;

public class VehicleMaintenanceAnalysisService(
    IGenerativeAiService aiService,
    IVehicleServiceClient vehicleServiceClient,
    ILogger<VehicleMaintenanceAnalysisService> logger) : IVehicleMaintenanceAnalysisService
{
    private readonly IGenerativeAiService _aiService = aiService;
    private readonly IVehicleServiceClient _vehicleServiceClient = vehicleServiceClient;
    private readonly ILogger<VehicleMaintenanceAnalysisService> _logger = logger;

    public async Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
        VehicleQuestionnaireRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch vehicle info from Vehicle Service
            _logger.LogInformation("Fetching vehicle info from Vehicle Service for UserVehicleId: {UserVehicleId}", request.UserVehicleId);
            var vehicleResponse = await _vehicleServiceClient.GetUserVehicleByIdAsync(request.UserVehicleId, cancellationToken);
            
            if (!vehicleResponse.IsSuccess || vehicleResponse.Data == null)
            {
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    vehicleResponse.Message ?? "Không thể lấy thông tin xe từ Vehicle Service");
            }

            var vehicle = vehicleResponse.Data;
            var vehicleInfo = vehicle.ToVehicleInfoDto();

            // Fetch default schedule from Vehicle Service
            _logger.LogInformation(
                "Fetching default schedule from Vehicle Service for VehicleModelId: {VehicleModelId}, PartCategoryCode: {PartCategoryCode}",
                request.VehicleModelId, request.PartCategoryCode);
            
            var scheduleResponse = await _vehicleServiceClient.GetDefaultScheduleAsync(
                request.VehicleModelId,
                request.PartCategoryCode,
                cancellationToken);

            if (!scheduleResponse.IsSuccess || scheduleResponse.Data == null)
            {
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    scheduleResponse.Message ?? "Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
            }

            var schedule = scheduleResponse.Data;
            var defaultSchedules = new List<DefaultScheduleDto>
            {
                schedule.ToDefaultScheduleDto(request.PartCategoryCode)
            };

            var prompt = PromptGenerator.CreateVehicleMaintenancePrompt(vehicleInfo, defaultSchedules, request.Answers);

            _logger.LogInformation("Generated prompt: {Prompt}", prompt);

            var aiResponse = await _aiService.GenerateContentAsync(
                prompt,
                AiOperation.GenerateText,
                request.UserId,
                temperature: 0.3m
            );

            if (!aiResponse.IsSuccess || aiResponse.Data == null)
            {
                _logger.LogError("AI service failed: {Content}", aiResponse.Data?.Content);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    aiResponse.Data?.Content ?? "Không thể phân tích dữ liệu");
            }

            var content = aiResponse.Data.Content;
            _logger.LogInformation("AI raw response: {Content}", content);

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

            // Validate: AI should return exactly 1 recommendation for 1 part request
            if (analysisResult.Recommendations.Count != 1)
            {
                _logger.LogWarning(
                    "AI returned {Count} recommendations, expected 1. Requested part: {PartCode}",
                    analysisResult.Recommendations.Count,
                    defaultSchedules.First().PartCategoryCode);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    $"AI trả về {analysisResult.Recommendations.Count} khuyến nghị thay vì 1. Vui lòng thử lại.");
            }

            var response = analysisResult.ToResponse(
                defaultSchedules,
                aiResponse.Data
            );

            _logger.LogInformation(
                "Successfully analyzed vehicle questionnaire - {Count} recommendations, {WarningCount} warnings",
                response.Recommendations.Count, response.Warnings.Count);

            return ApiResponse<VehicleQuestionnaireResponse>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing vehicle questionnaire");
            return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                "Có lỗi xảy ra khi phân tích dữ liệu");
        }
    }
}
