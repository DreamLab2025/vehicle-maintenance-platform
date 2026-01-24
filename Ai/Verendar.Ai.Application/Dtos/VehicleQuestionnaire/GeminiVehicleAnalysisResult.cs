using System.Text.Json.Serialization;

namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

/// <summary>
/// Raw JSON response from Gemini for vehicle maintenance analysis
/// This matches the JSON structure that Gemini returns
/// </summary>
public class GeminiVehicleAnalysisResult
{
    [JsonPropertyName("recommendations")]
    public List<GeminiPartRecommendation> Recommendations { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

public class GeminiPartRecommendation
{
    [JsonPropertyName("partCategoryCode")]
    public string PartCategoryCode { get; set; } = string.Empty;

    [JsonPropertyName("lastServiceOdometer")]
    public int? LastServiceOdometer { get; set; }

    [JsonPropertyName("lastServiceDate")]
    public string? LastServiceDate { get; set; }

    [JsonPropertyName("predictedNextOdometer")]
    public int? PredictedNextOdometer { get; set; }

    [JsonPropertyName("predictedNextDate")]
    public string? PredictedNextDate { get; set; }

    [JsonPropertyName("confidenceScore")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("needsImmediateAttention")]
    public bool NeedsImmediateAttention { get; set; }
}
