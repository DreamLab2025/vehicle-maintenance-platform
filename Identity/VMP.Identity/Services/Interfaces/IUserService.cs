using VMP.Common.Shared;
using VMP.Identity.Dtos;

namespace VMP.Identity.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    }
}
