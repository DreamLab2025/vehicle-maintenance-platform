using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Common.Caching;
using Verendar.Common.Databases.Base;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Entities;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Identity.Infrastructure.Services;

public class AuthService(
    ILogger<AuthService> logger,
    IUnitOfWork unitOfWork,
    IIdentityTokenService tokenService,
    ICacheService cacheService,
    IPublishEndpoint publishEndpoint) : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IIdentityTokenService _tokenService = tokenService;
    private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    private readonly ICacheService _cacheService = cacheService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    private string GetOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
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
            user.Status = EntityStatus.Inactive;
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var otpCode = GetOtpCode();
            _logger.LogInformation("Sending OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            await _cacheService.SetAsync($"otp_register:{request.Email}", otpCode, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Publishing OtpRequestedEvent for new user registration: {Email}", request.Email);
            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                Type = OtpType.Email
            });

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return ApiResponse<UserDto>.SuccessResponse(
                user.ToDto(),
                "Đăng ký người dùng thành công, vui lòng kiểm tra email để xác thực OTP");
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

    public async Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request)
    {
        try
        {
            var cacheKey = $"otp_register:{request.Email}";
            var storedOtp = await _cacheService.GetAsync<string>(cacheKey);

            if (string.IsNullOrEmpty(storedOtp))
            {
                return ApiResponse<bool>.FailureResponse("Mã OTP đã hết hạn hoặc không tồn tại.");
            }

            if (storedOtp != request.OtpCode)
            {
                return ApiResponse<bool>.FailureResponse("Mã OTP không chính xác.");
            }

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.Status == EntityStatus.Active)
            {
                return ApiResponse<bool>.SuccessResponse(true, "Tài khoản đã được kích hoạt trước đó.");
            }

            user.Status = EntityStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Publishing UserRegisteredEvent for user: {Email}", request.Email);
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                PhoneNumberVerified = user.PhoneNumberVerified,
                Email = user.Email,
                EmailVerified = user.EmailVerified,
                RegistrationDate = user.CreatedAt
            });

            await _cacheService.RemoveAsync(cacheKey);
            return ApiResponse<bool>.SuccessResponse(true, "Kích hoạt tài khoản thành công. Bạn có thể đăng nhập ngay bây giờ.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP: {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Lỗi hệ thống khi xác thực OTP.");
        }
    }

    public async Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("User not found for OTP resend: {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.Status == EntityStatus.Active)
            {
                _logger.LogInformation("OTP resend requested for already active user: {Email}", request.Email);
                return ApiResponse<bool>.SuccessResponse(true, "Tài khoản đã được kích hoạt trước đó.");
            }

            var lockKey = $"otp_resend_lock:{request.Email}";
            var isLocked = await _cacheService.GetAsync<bool?>(lockKey);
            if (isLocked == true)
            {
                _logger.LogInformation("OTP resend attempted too soon for user: {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây trước khi gửi lại OTP.");
            }

            var otpCode = GetOtpCode();
            _logger.LogInformation("Resending OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            await _cacheService.SetAsync($"otp_register:{request.Email}", otpCode, TimeSpan.FromMinutes(5));
            await _cacheService.SetAsync(lockKey, true, TimeSpan.FromSeconds(60));

            _logger.LogInformation("Publishing OtpRequestedEvent for resend OTP: {Email}", request.Email);
            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                Type = OtpType.Email
            });

            _logger.LogInformation("OTP code resent successfully to email: {Email}", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Gửi lại mã OTP thành công. Vui lòng kiểm tra email của bạn.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP: {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi trong quá trình gửi lại mã OTP.");
        }
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var lockKey = $"otp_forgot_lock:{request.Email}";
            var isLocked = await _cacheService.GetAsync<bool?>(lockKey);

            if (isLocked == true)
            {
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây trước khi yêu cầu lại.");
            }

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found for forgot password: {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            var otpCode = GetOtpCode();
            _logger.LogError("Send OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            await _cacheService.SetAsync($"otp_forgot:{request.Email}", otpCode, TimeSpan.FromMinutes(5));
            await _cacheService.SetAsync(lockKey, true, TimeSpan.FromSeconds(60));

            _logger.LogInformation("Publishing OtpRequestedEvent for forgot password: {Email}", request.Email);
            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                Type = OtpType.Email
            });

            _logger.LogInformation("Forgot password OTP code sent successfully to email: {Email}", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra email để tiếp tục quá trình đặt lại mật khẩu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ForgotPasswordAsync for email: {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var cacheKey = $"otp_forgot:{request.Email}";
            var storedOtp = await _cacheService.GetAsync<string>(cacheKey);

            if (string.IsNullOrEmpty(storedOtp) || storedOtp != request.OtpCode)
            {
                _logger.LogInformation("Invalid or expired OTP for password reset: {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found for password reset: {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogInformation("Password reset successfully for: {Email}", request.Email);

            return ApiResponse<bool>.SuccessResponse(true, "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi trong quá trình đặt lại mật khẩu.");
        }
    }
}
