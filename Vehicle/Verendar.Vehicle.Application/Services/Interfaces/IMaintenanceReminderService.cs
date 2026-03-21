namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceReminderService
    {
        Task<ApiResponse<List<ReminderDetailDto>>> GetRemindersAsync(Guid userId, Guid userVehicleId);
        Task SyncRemindersAsync(Guid vehicleId, int currentOdometer, Guid userId);
        Task PublishMaintenanceReminderIfNeededAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    }
}
