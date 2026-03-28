namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IConfidenceCalculationService
    {
        ConfidenceResult Calculate(ConfidenceInput input);
    }

    public class ConfidenceInput
    {
        public int OdometerEntryCount { get; set; }
        public double AiAdjustmentFactor { get; set; }
        public int ManufacturerKmInterval { get; set; }
        public int FinalPredictedOdo { get; set; }
        public int CurrentOdo { get; set; }
    }

    public class ConfidenceResult
    {
        public int Score { get; set; }
        public string Tier { get; set; } = "low";
        public List<ConfidenceBreakdownItem> Breakdown { get; set; } = [];
    }

    public class ConfidenceBreakdownItem
    {
        public string Name { get; set; } = string.Empty;
        public int Points { get; set; }
    }
}
