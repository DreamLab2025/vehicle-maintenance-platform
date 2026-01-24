namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire;

/// <summary>
/// AI analysis result for vehicle maintenance tracking
/// </summary>
public class VehicleQuestionnaireResponse
{
    /// <summary>
    /// Recommended tracking configuration for each part
    /// </summary>
    public List<PartTrackingRecommendation> Recommendations { get; set; } = new();

    /// <summary>
    /// Warnings about vehicle condition or overdue maintenance
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// AI metadata (tokens, cost, model used)
    /// </summary>
    public AiAnalysisMetadata Metadata { get; set; } = null!;
}

/// <summary>
/// AI recommendation for a specific part tracking
/// </summary>
public class PartTrackingRecommendation
{
    /// <summary>
    /// Part category code (e.g., "engine_oil")
    /// </summary>
    public string PartCategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Part category name (e.g., "Dầu động cơ")
    /// </summary>
    public string PartCategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Estimated last replacement odometer
    /// Null if unknown or not applicable
    /// </summary>
    public int? LastReplacementOdometer { get; set; }

    /// <summary>
    /// Estimated last replacement date
    /// Null if unknown or not applicable
    /// </summary>
    public DateOnly? LastReplacementDate { get; set; }

    /// <summary>
    /// Predicted next service odometer
    /// Calculated by AI based on interval
    /// </summary>
    public int? PredictedNextOdometer { get; set; }

    /// <summary>
    /// Predicted next service date
    /// Calculated by AI based on interval
    /// </summary>
    public DateOnly? PredictedNextDate { get; set; }

    /// <summary>
    /// AI confidence score (0.0 to 1.0)
    /// 1.0 = very confident, 0.5 = medium, 0.3 = low
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// AI reasoning/explanation for this recommendation
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Whether this part needs immediate attention
    /// </summary>
    public bool NeedsImmediateAttention { get; set; }
}

/// <summary>
/// Metadata about AI analysis
/// </summary>
public class AiAnalysisMetadata
{
    /// <summary>
    /// AI model used (e.g., "gemini-2.0-flash")
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Total tokens used (input + output)
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Total cost in USD
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }
}
