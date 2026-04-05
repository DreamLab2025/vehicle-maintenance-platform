using Verendar.Common.Stats;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class VehicleStatsService(ILogger<VehicleStatsService> logger, IUnitOfWork unitOfWork) : IVehicleStatsService
    {
        private readonly ILogger<VehicleStatsService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private const string Currency = "VND";

        public async Task<ApiResponse<VehicleOverviewStatsResponse>> GetOverviewStatsAsync(
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var toDate = to ?? today;
            var fromDate = from ?? toDate.AddMonths(-12);

            var vehicleCountTask = _unitOfWork.UserVehicles.CountAsync(
                x => x.CreatedAt >= fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                  && x.CreatedAt <= toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

            var recordSummaryTask = _unitOfWork.MaintenanceRecords.GetStatsSummaryAsync(fromDate, toDate, ct);
            var topCategoriesTask = _unitOfWork.MaintenanceRecordItems.GetTopPartCategoriesAsync(fromDate, toDate, 5, ct);
            var reminderByLevelTask = _unitOfWork.MaintenanceReminders.GetActiveCountByLevelAsync(ct);
            var activeReminderCountTask = _unitOfWork.MaintenanceReminders.CountAsync(
                x => x.Status == ReminderStatus.Active);

            await Task.WhenAll(vehicleCountTask, recordSummaryTask, topCategoriesTask, reminderByLevelTask, activeReminderCountTask);

            var vehicleCount = (int)vehicleCountTask.Result;
            var (totalRecords, totalCost) = recordSummaryTask.Result;
            var topCategories = topCategoriesTask.Result;
            var reminderByLevel = reminderByLevelTask.Result;
            var activeReminderCount = (int)activeReminderCountTask.Result;

            var avgCost = totalRecords > 0 ? Math.Round(totalCost / totalRecords, 0) : 0m;

            var response = new VehicleOverviewStatsResponse(
                UserVehicles: new UserVehiclesStatDto(vehicleCount),
                MaintenanceRecords: new MaintenanceRecordsStatDto(
                    Total: totalRecords,
                    TotalCost: totalCost,
                    Currency: Currency,
                    AvgCostPerRecord: avgCost),
                TopPartCategories: topCategories
                    .Select(c => new TopPartCategoryStatDto(c.PartCategoryId, c.Name, c.RecordCount, c.TotalCost))
                    .ToList(),
                Reminders: new MaintenanceRemindersStatDto(
                    Active: activeReminderCount,
                    ByLevel: new ReminderByLevelDto(
                        Normal: reminderByLevel.GetValueOrDefault(ReminderLevel.Normal),
                        Low: reminderByLevel.GetValueOrDefault(ReminderLevel.Low),
                        Medium: reminderByLevel.GetValueOrDefault(ReminderLevel.Medium),
                        High: reminderByLevel.GetValueOrDefault(ReminderLevel.High),
                        Critical: reminderByLevel.GetValueOrDefault(ReminderLevel.Critical)))
            );

            _logger.LogInformation("GetVehicleOverviewStats: from={From} to={To}", fromDate, toDate);
            return ApiResponse<VehicleOverviewStatsResponse>.SuccessResponse(response, "Vehicle stats retrieved successfully");
        }

        public async Task<ApiResponse<ChartTimelineResponse>> GetMaintenanceActivityChartAsync(
            ChartQueryRequest request,
            CancellationToken ct = default)
        {
            var error = request.Validate();
            if (error is not null)
                return ApiResponse<ChartTimelineResponse>.FailureResponse(error);

            var (from, to, groupBy) = request.Normalize();
            var points = await _unitOfWork.MaintenanceRecords.GetActivityChartAsync(from, to, groupBy, ct);

            var fromDate = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
            var toDate = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var response = new ChartTimelineResponse(groupBy, fromDate, toDate, points);
            return ApiResponse<ChartTimelineResponse>.SuccessResponse(response, "Chart data retrieved successfully");
        }
    }
}
