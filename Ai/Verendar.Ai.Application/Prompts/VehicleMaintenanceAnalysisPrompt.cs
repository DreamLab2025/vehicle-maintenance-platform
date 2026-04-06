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
            $"- Expected next service odometer (km): {predictionBase.ExpectedNextOdometer}\n" +
            $"- Expected next service date: {predictionBase.ExpectedNextDate:yyyy-MM-dd}\n" +
            $"- Data source: {predictionBase.DataSource}";

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
            return "Insufficient odometer history for personalized rate; using baseline analysis.";

        var kmPerMonth = summary.KmPerMonthLast3Months ?? summary.KmPerMonthAvg;
        return $"Observed driving rate: {kmPerMonth:F0} km/month ({summary.EntryCount} odometer updates).";
    }
}
