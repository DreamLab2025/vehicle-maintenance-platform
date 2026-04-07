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
            var earliest = currentOdo + (int)(kmInterval * 0.85);
            var latest = currentOdo + (int)(kmInterval * 1.15);

            return new PredictionBase
            {
                ExpectedNextOdometer = expected,
                ExpectedNextDate = expectedDate,
                EarliestNextOdometer = earliest,
                LatestNextOdometer = latest,
                KmIntervalUsed = kmInterval,
                DataSource = "manufacturer"
            };
        }

        private static PredictionBase ComputeState2(DateOnly today, int currentOdo, int kmInterval, double actualKmPerMonth)
        {
            var expected = currentOdo + kmInterval;
            var expectedMonths = kmInterval / actualKmPerMonth;
            var expectedDate = today.AddDays((int)(expectedMonths * 30));
            var earliest = currentOdo + (int)(kmInterval * 0.90);
            var latest = currentOdo + (int)(kmInterval * 1.10);

            return new PredictionBase
            {
                ExpectedNextOdometer = expected,
                ExpectedNextDate = expectedDate,
                EarliestNextOdometer = earliest,
                LatestNextOdometer = latest,
                KmIntervalUsed = kmInterval,
                DataSource = "actual_history"
            };
        }
    }
}
