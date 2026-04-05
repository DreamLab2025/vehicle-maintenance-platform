using System.Security.Cryptography;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Common.Caching;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Identity.Application.Services.Implements
{
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

        private static string GetOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString();
        }

        public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var existingUser = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration conflict: email already registered {Email}", email);
                return ApiResponse<UserDto>.ConflictResponse("Email đã được đăng ký");
            }

            var user = request.ToEntity(string.Empty);

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var otpCode = GetOtpCode();
            await _cacheService.SetAsync(CacheKeys.OtpRegister(email), otpCode, CacheKeys.OtpTtl);

            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.Add(CacheKeys.OtpTtl),
                Type = OtpType.Email
            });

            return ApiResponse<UserDto>.SuccessResponse(
                user.ToDto(),
                "Đăng ký người dùng thành công, vui lòng kiểm tra email để xác thực OTP");
        }

        public async Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Login: no user for email {Email}", email);
                return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login: invalid password for user {UserId}", user.Id);
                return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng");
            }

            if (!user.EmailVerified)
            {
                _logger.LogWarning("Login blocked: email not verified {Email}", email);
                return ApiResponse<TokenResponse>.FailureResponse(
                    "Tài khoản chưa được kích hoạt, vui lòng xác thực OTP",
                    403,
                    new { requiresOtpVerification = true, email });
            }

            var tokenResponse = _tokenService.GenerateTokens(user.ToTokenClaims());

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TokenResponse>.SuccessResponse(
                tokenResponse,
                "Đăng nhập thành công");
        }

        public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Refresh token: user not found {UserId}", userId);
                return ApiResponse<TokenResponse>.NotFoundResponse("Không tìm thấy người dùng");
            }

            if (!user.EmailVerified)
            {
                _logger.LogWarning("Refresh token: account not activated {UserId}", userId);
                return ApiResponse<TokenResponse>.ForbiddenResponse("Tài khoản người dùng chưa được kích hoạt");
            }

            if (string.IsNullOrWhiteSpace(user.RefreshToken) ||
                user.RefreshToken != refreshToken)
            {
                _logger.LogWarning("Refresh token: token mismatch {UserId}", userId);
                return ApiResponse<TokenResponse>.FailureResponse("Refresh token không hợp lệ");
            }

            if (user.RefreshTokenExpiryTime == null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token: expired {UserId}", userId);
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
            }
            else
            {
                tokenResponse.RefreshToken = user.RefreshToken;
            }

            return ApiResponse<TokenResponse>.SuccessResponse(
                tokenResponse,
                "Làm mới token thành công");
        }

        public async Task<ApiResponse<UserDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Change password: user not found {UserId}", userId);
                return ApiResponse<UserDto>.NotFoundResponse("Người dùng không tồn tại");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Change password: old password incorrect {UserId}", userId);
                return ApiResponse<UserDto>.FailureResponse("Mật khẩu cũ không đúng");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Đổi mật khẩu thành công");
        }

        public async Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var cacheKey = CacheKeys.OtpRegister(email);
            var storedOtp = await _cacheService.GetAsync<string>(cacheKey);

            if (string.IsNullOrEmpty(storedOtp))
            {
                _logger.LogWarning("Verify OTP: missing or expired cache entry {Email}", email);
                return ApiResponse<bool>.FailureResponse("Mã OTP đã hết hạn hoặc không tồn tại.");
            }

            if (storedOtp != request.OtpCode)
            {
                _logger.LogWarning("Verify OTP: code mismatch {Email}", email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không chính xác.");
            }

            await _cacheService.RemoveAsync(cacheKey);

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Verify OTP: user not found {Email}", email);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            if (user.EmailVerified)
            {
                _logger.LogWarning("Verify register OTP: already verified {Email}", email);
                return ApiResponse<bool>.SuccessResponse(true, "Tài khoản đã được kích hoạt trước đó.");
            }

            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                PhoneNumberVerified = user.PhoneNumberVerified,
                Email = user.Email,
                EmailVerified = user.EmailVerified,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender?.ToString(),
                RegistrationDate = user.CreatedAt
            });
            return ApiResponse<bool>.SuccessResponse(true, "Kích hoạt tài khoản thành công. Bạn có thể đăng nhập ngay bây giờ.");
        }

        public async Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("Resend OTP: user not found {Email}", email);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            if (user.EmailVerified)
            {
                _logger.LogWarning("Resend register OTP: already verified {Email}", email);
                return ApiResponse<bool>.SuccessResponse(true, "Tài khoản đã được kích hoạt trước đó.");
            }

            var lockKey = CacheKeys.OtpResendLock(email);
            var lockAcquired = await _cacheService.SetIfNotExistsAsync(lockKey, true, CacheKeys.OtpActionLockTtl);
            if (!lockAcquired)
            {
                _logger.LogWarning("Resend OTP: rate limited {Email}", email);
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây trước khi gửi lại OTP.");
            }

            var otpCode = GetOtpCode();
            await _cacheService.SetAsync(CacheKeys.OtpRegister(email), otpCode, CacheKeys.OtpTtl);

            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.Add(CacheKeys.OtpTtl),
                Type = OtpType.Email
            });

            return ApiResponse<bool>.SuccessResponse(true, "Gửi lại mã OTP thành công. Vui lòng kiểm tra email của bạn.");
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var lockKey = CacheKeys.OtpForgotLock(email);
            var lockAcquired = await _cacheService.SetIfNotExistsAsync(lockKey, true, CacheKeys.OtpActionLockTtl);
            if (!lockAcquired)
            {
                _logger.LogWarning("Forgot password: rate limited {Email}", email);
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây trước khi yêu cầu lại.");
            }

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Forgot password: user not found {Email}", email);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            var otpCode = GetOtpCode();
            await _cacheService.SetAsync(CacheKeys.OtpForgot(email), otpCode, CacheKeys.OtpTtl);

            await _publishEndpoint.Publish(new OtpRequestedEvent
            {
                UserId = user.Id,
                TargetValue = user.Email,
                Otp = otpCode,
                ExpiryTime = DateTime.UtcNow.Add(CacheKeys.OtpTtl),
                Type = OtpType.Email
            });

            return ApiResponse<bool>.SuccessResponse(true, "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra email để tiếp tục quá trình đặt lại mật khẩu.");
        }

        public async Task<ApiResponse<bool>> VerifyResetPasswordOtpAsync(VerifyResetPasswordOtpRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var cacheKey = CacheKeys.OtpForgot(email);
            var storedOtp = await _cacheService.GetAsync<string>(cacheKey);

            if (string.IsNullOrEmpty(storedOtp))
            {
                _logger.LogWarning("Verify reset password OTP: missing or expired cache entry {Email}", email);
                return ApiResponse<bool>.FailureResponse("Mã OTP đã hết hạn hoặc không tồn tại.");
            }

            if (storedOtp != request.OtpCode)
            {
                _logger.LogWarning("Verify reset password OTP: code mismatch {Email}", email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không chính xác.");
            }

            await _cacheService.RemoveAsync(cacheKey);
            await _cacheService.SetAsync(CacheKeys.OtpForgotVerified(email), true, CacheKeys.OtpForgotVerifiedTtl);

            return ApiResponse<bool>.SuccessResponse(true, "Xác thực OTP thành công. Vui lòng đặt lại mật khẩu trong 10 phút.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);
            var verifiedKey = CacheKeys.OtpForgotVerified(email);
            var isVerified = await _cacheService.GetAsync<bool>(verifiedKey);

            if (!isVerified)
            {
                _logger.LogWarning("Reset password: OTP not verified for {Email}", email);
                return ApiResponse<bool>.FailureResponse("Vui lòng xác thực OTP trước khi đặt lại mật khẩu.");
            }

            await _cacheService.RemoveAsync(verifiedKey);

            var user = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Reset password: user not found {Email}", email);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user.Id, user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.");
        }
    }
}
