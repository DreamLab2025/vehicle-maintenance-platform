using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;

namespace Verendar.Identity.Application.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
}
