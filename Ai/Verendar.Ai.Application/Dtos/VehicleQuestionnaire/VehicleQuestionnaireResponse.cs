namespace Verendar.Ai.Application.Dtos.VehicleQuestionnaire
{

    public class VehicleQuestionnaireResponse
    {
        public List<PartTrackingRecommendation> Recommendations { get; set; } = new();

        public List<string> Warnings { get; set; } = new();

        public AiAnalysisMetadata Metadata { get; set; } = null!;
    }

    public class PartTrackingRecommendation
    {
        public string PartCategorySlug { get; set; } = string.Empty;

        public int? LastReplacementOdometer { get; set; }

        public DateOnly? LastReplacementDate { get; set; }

        public int? PredictedNextOdometer { get; set; }

        public DateOnly? PredictedNextDate { get; set; }

        public double ConfidenceScore { get; set; }

        public string Reasoning { get; set; } = string.Empty;

        public bool NeedsImmediateAttention { get; set; }
    }

    public class AiAnalysisMetadata
    {
        public string Model { get; set; } = string.Empty;

        public int TotalTokens { get; set; }

        public decimal TotalCost { get; set; }

        public int ResponseTimeMs { get; set; }
    }
}
