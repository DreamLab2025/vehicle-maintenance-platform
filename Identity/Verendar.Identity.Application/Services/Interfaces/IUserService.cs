namespace Verendar.Identity.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
        Task<ApiResponse<CreateMechanicResponse>> CreateMechanicAsync(CreateMechanicRequest request);
        Task<ApiResponse<CreateManagerResponse>> CreateManagerAsync(CreateManagerRequest request);
        Task<ApiResponse<bool>> AssignRoleAsync(Guid userId, UserRole role);
        Task<ApiResponse<bool>> RevokeRoleAsync(Guid userId, UserRole role);
        Task<ApiResponse<bool>> BulkDeactivateAsync(IEnumerable<Guid> userIds);
    }
}
