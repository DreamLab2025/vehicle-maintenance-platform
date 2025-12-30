using Microsoft.AspNetCore.Identity;
using VMP.Common.Databases.Base;
using VMP.Common.Shared;
using VMP.Identity.Dtos;
using VMP.Identity.Entities;
using VMP.Identity.Mappings;
using VMP.Identity.Repositories.Interfaces;
using VMP.Identity.Services.Interfaces;

namespace VMP.Identity.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityTokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(
            ILogger<AuthService> logger,
            IUnitOfWork unitOfWork,
            IIdentityTokenService tokenService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserDto>.FailureResponse("Email đã được đăng ký");
                }

                var user = request.ToEntity(string.Empty);

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Email}", request.Email);

                return ApiResponse<UserDto>.SuccessResponse(
                    user.ToDto(),
                    "Đăng ký người dùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Email}", request.Email);
                return ApiResponse<UserDto>.FailureResponse("Đã xảy ra lỗi trong quá trình đăng ký");
            }
        }

        public async Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng");
                }

                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng");
                }

                if (user.Status != EntityStatus.Active)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Tài khoản người dùng chưa được kích hoạt");
                }

                var tokenResponse = _tokenService.GenerateTokens(user.ToTokenClaims());

                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = user.Id;

                await _unitOfWork.Users.UpdateAsync(user.Id, user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User logged in successfully: {Email}", request.Email);

                return ApiResponse<TokenResponse>.SuccessResponse(
                    tokenResponse,
                    "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Email}", request.Email);
                return ApiResponse<TokenResponse>.FailureResponse("Đã xảy ra lỗi trong quá trình đăng nhập");
            }
        }

        public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Không tìm thấy người dùng");
                }

                if (user.Status != EntityStatus.Active)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Tài khoản người dùng chưa được kích hoạt");
                }

                if (string.IsNullOrWhiteSpace(user.RefreshToken) ||
                    user.RefreshToken != refreshToken)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Refresh token không hợp lệ");
                }

                if (user.RefreshTokenExpiryTime == null ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return ApiResponse<TokenResponse>.FailureResponse("Refresh token đã hết hạn");
                }

                var tokenResponse = _tokenService.GenerateTokens(user.ToTokenClaims());
                var timeUntilExpiry = user.RefreshTokenExpiryTime.Value - DateTime.UtcNow;
                var shouldRotateRefreshToken = timeUntilExpiry.TotalDays < 2;

                if (shouldRotateRefreshToken)
                {
                    user.RefreshToken = tokenResponse.RefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = user.Id;

                    await _unitOfWork.Users.UpdateAsync(user.Id, user);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Token and refresh token rotated for user: {UserId}, expires in {Days} days",
                        userId, timeUntilExpiry.TotalDays);
                }
                else
                {
                    tokenResponse.RefreshToken = user.RefreshToken;

                    _logger.LogInformation("Access token refreshed for user: {UserId}, refresh token still valid for {Days} days",
                        userId, timeUntilExpiry.TotalDays);
                }

                return ApiResponse<TokenResponse>.SuccessResponse(
                    tokenResponse,
                    "Làm mới token thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for user: {UserId}", userId);
                return ApiResponse<TokenResponse>.FailureResponse("Đã xảy ra lỗi trong quá trình làm mới token");
            }
        }

        public async Task<ApiResponse<UserDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for password change: {UserId}", userId);
                    return ApiResponse<UserDto>.FailureResponse("Người dùng không tồn tại");
                }
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<UserDto>.FailureResponse("Mật khẩu cũ không đúng");
                }
                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

                await _unitOfWork.Users.UpdateAsync(user.Id, user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
                return ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return ApiResponse<UserDto>.FailureResponse("Đã xảy ra lỗi trong quá trình đổi mật khẩu");
            }
        }
    }
}
