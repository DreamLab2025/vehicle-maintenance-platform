using Verender.Common.Shared;
using Verender.Identity.Dtos;

namespace Verender.Identity.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    }
}
