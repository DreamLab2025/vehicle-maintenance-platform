using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;

namespace VMP.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleTypeService
    {
        Task<ApiResponse<List<TypeResponse>>> GetAllTypesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request);
        Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request);
        Task<ApiResponse<string>> DeleteTypeAsync(Guid id);
    }
}
