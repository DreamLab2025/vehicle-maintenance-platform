using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Helpers;
using Verendar.Ai.Application.Mappings;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Services.Implements;

public class VehicleMaintenanceAnalysisService : IVehicleMaintenanceAnalysisService
{
    private readonly IGenerativeAiService _aiService;
    private readonly ILogger<VehicleMaintenanceAnalysisService> _logger;

    public VehicleMaintenanceAnalysisService(
        IGenerativeAiService aiService,
        ILogger<VehicleMaintenanceAnalysisService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
        VehicleQuestionnaireRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate: Only 1 part category per request for accurate analysis
            if (request.DefaultSchedules == null || request.DefaultSchedules.Count == 0)
            {
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "Vui lòng cung cấp lịch bảo dưỡng cho ít nhất 1 linh kiện");
            }

            if (request.DefaultSchedules.Count > 1)
            {
                _logger.LogWarning(
                    "Multiple part categories in request ({Count}). Recommend sending 1 part per request for accuracy.",
                    request.DefaultSchedules.Count);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "Chỉ hỗ trợ phân tích 1 linh kiện trong mỗi request. Vui lòng gửi riêng cho từng linh kiện.");
            }

            var prompt = PromptGenerator.CreateVehicleMaintenancePrompt(request.VehicleInfo, request.DefaultSchedules, request.Answers);

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
                    request.DefaultSchedules.First().PartCategoryCode);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    $"AI trả về {analysisResult.Recommendations.Count} khuyến nghị thay vì 1. Vui lòng thử lại.");
            }

            var response = analysisResult.ToResponse(
                request.DefaultSchedules,
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
