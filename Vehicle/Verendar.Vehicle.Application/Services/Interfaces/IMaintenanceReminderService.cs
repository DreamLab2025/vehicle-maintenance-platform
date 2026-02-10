namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceReminderService
    {
        Task PublishMaintenanceReminderIfNeededAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    }
}
