using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Ai.Application.Services.Interfaces;

namespace Verendar.Ai.Application.Prompts;

public static class VehicleMaintenanceAnalysisPrompt
{
    public static string ApplyTemplate(
        string template,
        VehicleInfoDto vehicleInfo,
        DefaultScheduleDto schedule,
        IEnumerable<QuestionAnswerDto>? answers,
        PredictionBase predictionBase,
        VehicleServiceOdometerSummaryResponse? odometerSummary)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var scheduleBlock =
            $"ITEM: {{ \"Slug\": \"{schedule.PartCategorySlug}\", \"Initial_Km\": {schedule.InitialKm}, \"Interval_Km\": {schedule.KmInterval}, \"Interval_Month\": {schedule.MonthsInterval} }}";

        var answerBlock = (answers != null && answers.Any())
            ? string.Join("\n", answers.Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => $"- User Input ({a.Question}): \"{a.Value}\""))
            : "No user input provided.";

        var vehicleName = $"{vehicleInfo.Brand} {vehicleInfo.Model}".Trim();

        var basePredictionBlock =
            $"- Odometer dự kiến: {predictionBase.ExpectedNextOdometer} km\n" +
            $"- Ngày dự kiến: {predictionBase.ExpectedNextDate:yyyy-MM-dd}\n" +
            $"- Nguồn dữ liệu: {predictionBase.DataSource}";

        var drivingPatternBlock = BuildDrivingPatternBlock(odometerSummary);

        return template
            .Replace("[[TODAY]]", today, StringComparison.Ordinal)
            .Replace("[[VEHICLE_NAME]]", vehicleName, StringComparison.Ordinal)
            .Replace("[[CURRENT_ODO]]", vehicleInfo.CurrentOdometer.ToString(), StringComparison.Ordinal)
            .Replace("[[PURCHASE_DATE]]", vehicleInfo.PurchaseDate.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("[[SCHEDULE_BLOCK]]", scheduleBlock, StringComparison.Ordinal)
            .Replace("[[ANSWER_BLOCK]]", answerBlock, StringComparison.Ordinal)
            .Replace("[[PART_CATEGORY_SLUG]]", schedule.PartCategorySlug, StringComparison.Ordinal)
            .Replace("[[BASE_PREDICTION]]", basePredictionBlock, StringComparison.Ordinal)
            .Replace("[[DRIVING_PATTERN]]", drivingPatternBlock, StringComparison.Ordinal);
    }

    private static string BuildDrivingPatternBlock(VehicleServiceOdometerSummaryResponse? summary)
    {
        if (summary == null || summary.EntryCount < 2)
            return "Người dùng chưa có lịch sử odometer đủ. Đây là phân tích baseline.";

        var kmPerMonth = summary.KmPerMonthLast3Months ?? summary.KmPerMonthAvg;
        return $"Tốc độ lái thực tế: {kmPerMonth:F0} km/tháng ({summary.EntryCount} lần cập nhật).";
    }
}
