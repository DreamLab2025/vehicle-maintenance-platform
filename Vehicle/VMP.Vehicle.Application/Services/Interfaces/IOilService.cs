using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Services.Interfaces
{
    public interface IOilService
    {
        Task<ApiResponse<List<OilResponse>>> GetAllOilsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<OilResponse>> GetOilByIdAsync(Guid id);
        Task<ApiResponse<OilResponse>> GetOilByVehiclePartIdAsync(Guid vehiclePartId);
        Task<ApiResponse<List<OilResponse>>> GetOilsByVehicleUsageAsync(OilVehicleUsage vehicleUsage);
        Task<ApiResponse<List<OilResponse>>> GetOilsByViscosityGradeAsync(string viscosityGrade);
        Task<ApiResponse<OilResponse>> CreateOilAsync(OilRequest request);
        Task<ApiResponse<OilResponse>> UpdateOilAsync(Guid id, OilRequest request);
        Task<ApiResponse<string>> DeleteOilAsync(Guid id);
    }
}
