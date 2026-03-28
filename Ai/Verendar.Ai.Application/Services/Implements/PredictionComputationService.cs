using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Ai.Application.Services.Interfaces;

namespace Verendar.Ai.Application.Services.Implements
{
    public class PredictionComputationService : IPredictionComputationService
    {
        public PredictionBase ComputeBase(
            VehicleInfoDto vehicleInfo,
            DefaultScheduleDto schedule,
            VehicleServiceOdometerSummaryResponse? odometerSummary)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentOdo = vehicleInfo.CurrentOdometer;
            var kmInterval = schedule.KmInterval;

            var actualKmPerMonth = odometerSummary?.KmPerMonthLast3Months ?? odometerSummary?.KmPerMonthAvg;
            var hasHistory = odometerSummary?.EntryCount >= 2 && actualKmPerMonth is > 0;

            if (hasHistory)
                return ComputeState2(today, currentOdo, kmInterval, actualKmPerMonth!.Value);

            return ComputeState1(today, currentOdo, kmInterval, schedule.MonthsInterval);
        }

        private static PredictionBase ComputeState1(DateOnly today, int currentOdo, int kmInterval, int monthsInterval)
        {
            var expected = currentOdo + kmInterval;
            var expectedDate = today.AddMonths(monthsInterval);

            return new PredictionBase
            {
                ExpectedNextOdometer = expected,
                ExpectedNextDate = expectedDate,
                EarliestNextOdometer = (int)(expected * 0.85),
                LatestNextOdometer = (int)(expected * 1.15),
                KmIntervalUsed = kmInterval,
                DataSource = "manufacturer"
            };
        }

        private static PredictionBase ComputeState2(DateOnly today, int currentOdo, int kmInterval, double actualKmPerMonth)
        {
            var expected = currentOdo + kmInterval;
            var expectedMonths = kmInterval / actualKmPerMonth;
            var expectedDate = today.AddDays((int)(expectedMonths * 30));

            return new PredictionBase
            {
                ExpectedNextOdometer = expected,
                ExpectedNextDate = expectedDate,
                EarliestNextOdometer = (int)(expected * 0.90),
                LatestNextOdometer = (int)(expected * 1.10),
                KmIntervalUsed = kmInterval,
                DataSource = "actual_history"
            };
        }
    }
}
