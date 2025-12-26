using VMP.Common.Shared;
using VMP.Identity.Dtos;

namespace VMP.Identity.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request);
        Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request);
        Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken);
    }
}
