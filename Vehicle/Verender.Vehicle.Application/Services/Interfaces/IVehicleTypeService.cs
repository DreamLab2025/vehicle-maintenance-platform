using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;

namespace Verender.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleTypeService
    {
        Task<ApiResponse<List<TypeResponse>>> GetAllTypesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request);
        Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request);
        Task<ApiResponse<string>> DeleteTypeAsync(Guid id);
    }
}
