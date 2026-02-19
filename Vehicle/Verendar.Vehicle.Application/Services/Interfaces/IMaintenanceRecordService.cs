using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceRecordService
    {
        Task<ApiResponse<CreateMaintenanceRecordResponse>> CreateMaintenanceRecordAsync(Guid userId, Guid vehicleId, CreateMaintenanceRecordRequest request);
    }
}
