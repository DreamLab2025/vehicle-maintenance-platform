using Verendar.Ai.Application.Dtos.OdometerScan;

namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IOdometerScanService
    {
        Task<ApiResponse<OdometerScanResponse>> ScanOdometerAsync(
            Guid userId,
            OdometerScanRequest request,
            CancellationToken cancellationToken = default);
    }
}
