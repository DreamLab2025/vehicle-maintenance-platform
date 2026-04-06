namespace Verendar.Ai.Application.Mappings
{
    public static class VehicleQuestionnaireMappings
    {
        public static AiAnalysisMetadata ToAiAnalysisMetadata(this GenerativeAiResponse aiResponse)
        {
            return new AiAnalysisMetadata
            {
                Model = aiResponse.Model,
                TotalTokens = aiResponse.TotalTokens,
                TotalCost = aiResponse.TotalCost,
                ResponseTimeMs = aiResponse.ResponseTimeMs
            };
        }
    }
}
