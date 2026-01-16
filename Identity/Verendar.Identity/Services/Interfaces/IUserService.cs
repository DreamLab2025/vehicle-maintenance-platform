using Verendar.Common.Shared;
using Verendar.Identity.Dtos;

namespace Verendar.Identity.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    }
}
