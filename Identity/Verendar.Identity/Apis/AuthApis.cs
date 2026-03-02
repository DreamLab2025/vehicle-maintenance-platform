using Verendar.Common.EndpointFilters;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Services.Interfaces;

namespace Verendar.Identity.Apis
{
    public static class AuthApis
    {
        public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/auth")
                .MapAuthRoutes()
                .WithTags("Authentication Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapAuthRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/login", LoginUser)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<LoginRequest>())
                .WithName("Login")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đăng nhập người dùng";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest);

            group.MapPost("/register", RegisterUser)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<RegisterRequest>())
                .WithName("Register")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đăng ký người dùng mới";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest);

            group.MapPost("/refresh-token", RefreshToken)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<RefreshTokenRequest>())
                .WithName("RefreshToken")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Làm mới access token";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/change-password", ChangePassword)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ChangePasswordRequest>())
                .WithName("ChangePassword")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đổi mật khẩu người dùng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/verify-otp", VerifyOtp)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<VerifyOtpRequest>())
                .WithName("VerifyOtp")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xác thực OTP đăng ký";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            group.MapPost("/resend-otp", ResendOtp)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ResendOtpRequest>())
                .WithName("ResendOtp")
                .WithOpenApi(op =>
                {
                    op.Summary = "Gửi lại mã OTP (Giới hạn 60s/lần)";
                    return op;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            group.MapPost("/forgot-password", ForgotPassword)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ForgotPasswordRequest>())
                .WithName("ForgotPassword")
                .WithOpenApi(op =>
                {
                    op.Summary = "Yêu cầu mã OTP lấy lại mật khẩu";
                    return op;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

            group.MapPost("/reset-password", ResetPassword)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ResetPasswordRequest>())
                .WithName("ResetPassword")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đặt lại mật khẩu người dùng";
                    return operation;
                })
                .AllowAnonymous()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> ForgotPassword(ForgotPasswordRequest request, IAuthService authService)
        {
            var result = await authService.ForgotPasswordAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> ResetPassword(
            ResetPasswordRequest request,
            IAuthService authService)
        {
            var result = await authService.ResetPasswordAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> ResendOtp(ResendOtpRequest request, IAuthService authService)
        {
            var result = await authService.ResendRegisterOtpAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> VerifyOtp(VerifyOtpRequest request, IAuthService authService)
        {
            var result = await authService.VerifyRegisterOtpAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> ChangePassword(ChangePasswordRequest request, IAuthService authService, ICurrentUserService currentUserService)
        {
            var userId = currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await authService.ChangePasswordAsync(userId, request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> RefreshToken(
            RefreshTokenRequest request,
            ICurrentUserService currentUser,
            IAuthService authService)
        {
            var userId = currentUser.UserId;

            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await authService.RefreshTokenAsync(userId, request.RefreshToken);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> RegisterUser(
            RegisterRequest request,
            IAuthService authService)
        {
            var result = await authService.RegisterUserAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }

        private static async Task<IResult> LoginUser(
            LoginRequest request,
            IAuthService authService)
        {
            var result = await authService.LoginUserAsync(request);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(result);
        }
    }
}
