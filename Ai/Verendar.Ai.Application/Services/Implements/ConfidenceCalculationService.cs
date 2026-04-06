using Verendar.Ai.Application.Services.Interfaces;

namespace Verendar.Ai.Application.Services.Implements
{
    public class ConfidenceCalculationService : IConfidenceCalculationService
    {
        public ConfidenceResult Calculate(ConfidenceInput input)
        {
            var breakdown = new List<ConfidenceBreakdownItem>();
            var score = 35;
            breakdown.Add(new ConfidenceBreakdownItem { Name = "base_score", Points = 35 });

            if (input.OdometerEntryCount >= 2)
            {
                score += 15;
                breakdown.Add(new ConfidenceBreakdownItem { Name = "has_odometer_history", Points = 15 });
            }

            if (input.OdometerEntryCount >= 5)
            {
                score += 10;
                breakdown.Add(new ConfidenceBreakdownItem { Name = "5_or_more_entries", Points = 10 });
            }

            if (input.OdometerEntryCount >= 10)
            {
                score += 10;
                breakdown.Add(new ConfidenceBreakdownItem { Name = "10_or_more_entries", Points = 10 });
            }

            if (input.ManufacturerKmInterval > 0)
            {
                var deviation = Math.Abs(input.FinalPredictedOdo - (input.CurrentOdo + input.ManufacturerKmInterval))
                                / (double)input.ManufacturerKmInterval;
                if (deviation < 0.20)
                {
                    score += 10;
                    breakdown.Add(new ConfidenceBreakdownItem { Name = "prediction_close_to_manufacturer", Points = 10 });
                }
            }

            score = Math.Clamp(score, 0, 90);

            var tier = score switch
            {
                < 50 => "low",
                < 75 => "medium",
                _ => "high"
            };

            return new ConfidenceResult
            {
                Score = score,
                Tier = tier,
                Breakdown = breakdown
            };
        }
    }
}
