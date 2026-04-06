using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Vehicle.Application.Services.Implements;

public class MaintenanceReminderLookupService(IUnitOfWork unitOfWork) : IMaintenanceReminderLookupService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<IReadOnlyList<MaintenanceReminderLookupItemResponse>>> LookupForNotificationAsync(
        MaintenanceReminderLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ReminderIds == null || request.ReminderIds.Count == 0)
            return ApiResponse<IReadOnlyList<MaintenanceReminderLookupItemResponse>>.SuccessResponse(
                [], "Không có reminder.");

        var rows = await _unitOfWork.MaintenanceReminders.GetByIdsForUserAsync(
            request.ReminderIds,
            request.UserId,
            cancellationToken);

        var list = rows
            .Select(r =>
            {
                var pt = r.TrackingCycle.PartTracking;
                return new MaintenanceReminderLookupItemResponse
                {
                    ReminderId = r.Id,
                    PartTrackingId = pt.Id,
                    ReminderStatus = r.Status.ToString(),
                    LastReplacementDate = pt.LastReplacementDate,
                    LastReplacementOdometer = pt.LastReplacementOdometer
                };
            })
            .ToList();

        return ApiResponse<IReadOnlyList<MaintenanceReminderLookupItemResponse>>.SuccessResponse(
            list,
            "Tra cứu reminder thành công.");
    }
}
