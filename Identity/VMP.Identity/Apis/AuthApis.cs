using VMP.Common.Jwt;
using VMP.Common.Shared;
using VMP.Identity.Dtos;
using VMP.Identity.Services.Interfaces;

namespace VMP.Identity.Apis
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
                .WithName("Login")
                .WithSummary("Đăng nhập người dùng")
                .WithDescription("Xác thực người dùng bằng email và password, trả về access token và refresh token")
                .AllowAnonymous()
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest);

            group.MapPost("/register", RegisterUser)
                .WithName("Register")
                .WithSummary("Đăng ký người dùng mới")
                .WithDescription("Tạo tài khoản người dùng mới với email và password")
                .AllowAnonymous()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest);

            group.MapPost("/refresh-token", RefreshToken)
                .WithName("RefreshToken")
                .WithSummary("Làm mới access token")
                .WithDescription("Sử dụng refresh token để lấy access token mới. Cần có JWT token hợp lệ trong Authorization header")
                .RequireAuthorization()
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/change-password", ChangePassword)
                .WithName("ChangePassword")
                .WithSummary("Đổi mật khẩu người dùng")
                .WithDescription("Cho phép người dùng đã xác thực đổi mật khẩu của họ")
                .RequireAuthorization()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
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
