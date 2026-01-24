using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Prompts;
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
            // Validate request
            if (request.VehicleInfo == null)
            {
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "Thông tin xe không hợp lệ");
            }

            if (!request.DefaultSchedules.Any())
            {
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "Chưa có lịch bảo dưỡng mặc định");
            }

            _logger.LogInformation(
                "Analyzing vehicle questionnaire for user {UserId}, vehicle {VehicleId}",
                request.UserId, request.UserVehicleId);

            // Build prompt using VehicleMaintenancePromptBuilder
            var promptBuilder = new VehicleMaintenancePromptBuilder(request.VehicleInfo);

            // Add default schedules
            foreach (var schedule in request.DefaultSchedules)
            {
                promptBuilder.AddSchedules(request.DefaultSchedules);
            }

            // Add user answers
            foreach (var answer in request.Answers)
            {
                promptBuilder.AddAnswers(request.Answers);
            }

            var prompt = promptBuilder.Build();

            _logger.LogDebug("Generated prompt: {Prompt}", prompt);

            // Call AI service
            var aiResponse = await _aiService.GenerateContentAsync(
                prompt,
                AiOperation.GenerateText,
                request.UserId,
                temperature: 0.3m
            );

            if (!aiResponse.IsSuccess || aiResponse.Data == null)
            {
                _logger.LogError("AI service failed: {Message}", aiResponse.Message);
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    aiResponse.Message ?? "Không thể phân tích dữ liệu");
            }

            var content = aiResponse.Data.Content;
            _logger.LogDebug("AI raw response: {Content}", content);

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
                    "Không thể phân tích kết quả từ AI");
            }

            if (analysisResult == null || !analysisResult.Recommendations.Any())
            {
                _logger.LogWarning("AI returned no recommendations");
                return ApiResponse<VehicleQuestionnaireResponse>.FailureResponse(
                    "AI không thể đưa ra khuyến nghị");
            }

            var response = new VehicleQuestionnaireResponse
            {
                Recommendations = analysisResult.Recommendations.Select(r => new PartTrackingRecommendation
                {
                    PartCategoryCode = r.PartCategoryCode,
                    PartCategoryName = GetPartCategoryName(r.PartCategoryCode, request.DefaultSchedules),
                    LastReplacementOdometer = r.LastServiceOdometer,
                    LastReplacementDate = ParseDateOnly(r.LastServiceDate),
                    PredictedNextOdometer = r.PredictedNextOdometer,
                    PredictedNextDate = ParseDateOnly(r.PredictedNextDate),
                    ConfidenceScore = r.ConfidenceScore,
                    Reasoning = r.Reasoning,
                    NeedsImmediateAttention = r.NeedsImmediateAttention
                }).ToList(),
                Warnings = analysisResult.Warnings,
                Metadata = new AiAnalysisMetadata
                {
                    Model = aiResponse.Data.Model,
                    TotalTokens = aiResponse.Data.TotalTokens,
                    TotalCost = aiResponse.Data.TotalCost,
                    ResponseTimeMs = aiResponse.Data.ResponseTimeMs
                }
            };

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

    private static string GetPartCategoryName(string partCode, List<DefaultScheduleDto> schedules)
    {
        return schedules.FirstOrDefault(s => s.PartCategoryCode == partCode)?.PartCategoryName ?? partCode;
    }

    private static DateOnly? ParseDateOnly(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        if (DateOnly.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        if (DateOnly.TryParse(dateString, out date))
            return date;

        return null;
    }
}
