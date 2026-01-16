using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleTypeService
    {
        Task<ApiResponse<List<TypeResponse>>> GetAllTypesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request);
        Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request);
        Task<ApiResponse<string>> DeleteTypeAsync(Guid id);
    }
}
