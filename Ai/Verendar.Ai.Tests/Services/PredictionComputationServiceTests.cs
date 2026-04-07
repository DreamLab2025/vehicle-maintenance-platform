namespace Verendar.Ai.Tests.Services;

using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Ai.Application.Services.Implements;

public class PredictionComputationServiceTests
{
    [Fact]
    public void ComputeBase_State1_ShouldKeepRangeAroundIntervalNotAbsoluteOdometer()
    {
        var sut = new PredictionComputationService();
        var vehicleInfo = new VehicleInfoDto
        {
            CurrentOdometer = 700_000,
            PurchaseDate = DateTime.UtcNow.AddYears(-3)
        };
        var schedule = new DefaultScheduleDto
        {
            KmInterval = 2_700,
            MonthsInterval = 1
        };

        var result = sut.ComputeBase(vehicleInfo, schedule, odometerSummary: null);

        result.ExpectedNextOdometer.Should().Be(702_700);
        result.EarliestNextOdometer.Should().Be(702_295);
        result.LatestNextOdometer.Should().Be(703_104);
        result.EarliestNextOdometer.Should().BeGreaterThanOrEqualTo(vehicleInfo.CurrentOdometer);
    }

    [Fact]
    public void ComputeBase_State2_ShouldUseHistoryRangeBasedOnInterval()
    {
        var sut = new PredictionComputationService();
        var vehicleInfo = new VehicleInfoDto
        {
            CurrentOdometer = 700_000,
            PurchaseDate = DateTime.UtcNow.AddYears(-3)
        };
        var schedule = new DefaultScheduleDto
        {
            KmInterval = 2_700,
            MonthsInterval = 1
        };
        var odometerSummary = new VehicleServiceOdometerSummaryResponse
        {
            EntryCount = 3,
            KmPerMonthLast3Months = 900
        };

        var result = sut.ComputeBase(vehicleInfo, schedule, odometerSummary);

        result.ExpectedNextOdometer.Should().Be(702_700);
        result.EarliestNextOdometer.Should().Be(702_430);
        result.LatestNextOdometer.Should().Be(702_970);
        result.DataSource.Should().Be("actual_history");
    }
}
