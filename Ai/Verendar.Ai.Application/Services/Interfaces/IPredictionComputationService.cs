using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Dtos.VehicleService;

namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IPredictionComputationService
    {
        PredictionBase ComputeBase(
            VehicleInfoDto vehicleInfo,
            DefaultScheduleDto schedule,
            VehicleServiceOdometerSummaryResponse? odometerSummary);
    }

    public class PredictionBase
    {
        public int ExpectedNextOdometer { get; set; }
        public DateOnly ExpectedNextDate { get; set; }
        public int EarliestNextOdometer { get; set; }
        public int LatestNextOdometer { get; set; }
        public int KmIntervalUsed { get; set; }

        public string DataSource { get; set; } = "manufacturer";
    }
}
