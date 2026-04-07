namespace Verendar.Identity.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(UserFilterRequest request);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
        Task<ApiResponse<CreateMechanicResponse>> CreateMechanicAsync(CreateMechanicRequest request);
        Task<ApiResponse<CreateManagerResponse>> CreateManagerAsync(CreateManagerRequest request);
        Task<ApiResponse<bool>> AssignRoleAsync(Guid userId, UserRole role);
        Task<ApiResponse<bool>> RevokeRoleAsync(Guid userId, UserRole role);
        Task<ApiResponse<bool>> BulkDeactivateAsync(IEnumerable<Guid> userIds);
        Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default);
        Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default);
        Task<ApiResponse<bool>> DeleteUserAsync(Guid userId, Guid actingUserId, CancellationToken ct = default);
    }
}
