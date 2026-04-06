namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface ITypeService
    {
        Task<ApiResponse<List<TypeSummary>>> GetAllTypesAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<TypeResponse>> GetTypeByIdAsync(Guid id);
        Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request);
        Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request);
        Task<ApiResponse<string>> DeleteTypeAsync(Guid id);
    }
}
