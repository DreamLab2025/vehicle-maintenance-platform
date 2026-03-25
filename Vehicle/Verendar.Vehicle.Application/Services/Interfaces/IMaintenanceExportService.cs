namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceExportService
    {
        Task<ApiResponse<(byte[] Data, string ContentType, string FileName)>> ExportAsync(
            Guid userId,
            ExportMaintenanceRequest request,
            CancellationToken cancellationToken = default);
    }
}
