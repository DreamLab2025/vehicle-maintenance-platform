namespace Verendar.Identity.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request);
        Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request);
        Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken);
        Task<ApiResponse<UserDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request);
        Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> VerifyResetPasswordOtpAsync(VerifyResetPasswordOtpRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
