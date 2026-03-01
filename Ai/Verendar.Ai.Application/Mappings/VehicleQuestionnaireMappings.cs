using System.Globalization;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

namespace Verendar.Ai.Application.Mappings
{
    public static class VehicleQuestionnaireMappings
    {
        public static VehicleQuestionnaireResponse ToResponse(
            this GeminiVehicleAnalysisResult analysisResult,
            GenerativeAiResponse? aiResponse = null)
        {
            return new VehicleQuestionnaireResponse
            {
                Recommendations = [.. analysisResult.Recommendations.Select(r => r.ToRecommendationDto())],

                Warnings = [.. analysisResult.Warnings],

                Metadata = ToAiAnalysisMetadata(aiResponse)
            };
        }

        private static AiAnalysisMetadata ToAiAnalysisMetadata(this GenerativeAiResponse? aiResponse)
        {
            return new AiAnalysisMetadata
            {
                Model = aiResponse!.Model,
                TotalTokens = aiResponse.TotalTokens,
                TotalCost = aiResponse.TotalCost,
                ResponseTimeMs = aiResponse.ResponseTimeMs
            };
        }

        private static PartTrackingRecommendation ToRecommendationDto(
            this GeminiPartRecommendation source)
        {
            return new PartTrackingRecommendation
            {
                PartCategoryCode = source.PartCategoryCode,

                LastReplacementOdometer = source.LastServiceOdometer,
                LastReplacementDate = ParseDateOnly(source.LastServiceDate),

                PredictedNextOdometer = source.PredictedNextOdometer,
                PredictedNextDate = ParseDateOnly(source.PredictedNextDate),

                ConfidenceScore = source.ConfidenceScore,
                Reasoning = source.Reasoning,
                NeedsImmediateAttention = source.NeedsImmediateAttention
            };
        }

        private static DateOnly? ParseDateOnly(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;

            if (DateOnly.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            if (DateOnly.TryParse(dateString, out date))
                return date;

            return null;
        }
    }
}