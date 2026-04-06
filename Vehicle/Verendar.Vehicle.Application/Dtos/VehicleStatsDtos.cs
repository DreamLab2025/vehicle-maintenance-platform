namespace Verendar.Vehicle.Application.Dtos
{
    public record VehicleOverviewStatsResponse(
        UserVehiclesStatDto UserVehicles,
        MaintenanceRecordsStatDto MaintenanceRecords,
        List<TopPartCategoryStatDto> TopPartCategories,
        MaintenanceRemindersStatDto Reminders
    );

    public record UserVehiclesStatDto(int Total);

    public record MaintenanceRecordsStatDto(
        int Total,
        decimal TotalCost,
        string Currency,
        decimal AvgCostPerRecord
    );

    public record TopPartCategoryStatDto(
        Guid PartCategoryId,
        string Name,
        int RecordCount,
        decimal TotalCost
    );

    public record MaintenanceRemindersStatDto(
        int Active,
        ReminderByLevelDto ByLevel
    );

    public record ReminderByLevelDto(
        int Normal,
        int Low,
        int Medium,
        int High,
        int Critical
    );
}
