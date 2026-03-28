using System.Text.Json.Serialization;

namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire
{
    public class GeminiVehicleAnalysisResult
    {
        [JsonPropertyName("recommendations")]
        public List<GeminiPartRecommendation> Recommendations { get; set; } = new();
    }

    public class GeminiPartRecommendation
    {
        [JsonPropertyName("partCategorySlug")]
        public string PartCategorySlug { get; set; } = string.Empty;

        [JsonPropertyName("lastServiceOdometer")]
        public int? LastServiceOdometer { get; set; }

        [JsonPropertyName("lastServiceDate")]
        public string? LastServiceDate { get; set; }

        [JsonPropertyName("usageAdjustmentFactor")]
        public double UsageAdjustmentFactor { get; set; } = 1.0;

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; } = string.Empty;

        [JsonPropertyName("needsImmediateAttention")]
        public bool NeedsImmediateAttention { get; set; }

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();
    }
}
