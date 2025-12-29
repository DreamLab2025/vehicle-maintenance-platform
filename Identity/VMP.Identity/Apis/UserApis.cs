using VMP.Common.Jwt;
using VMP.Common.Shared;
using VMP.Identity.Dtos;
using VMP.Identity.Entities;
using VMP.Identity.Services.Interfaces;

namespace VMP.Identity.Apis
{
    public static class UserApis
    {
        public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/users")
                .MapUserRoutes()
                .WithTags("User Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapUserRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllUsers)
                .WithName("GetAllUsers")
                .WithSummary("Lấy danh sách tất cả người dùng")
                .WithDescription("Trả về danh sách tất cả người dùng trong hệ thống")
                .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
                .Produces<ApiResponse<List<UserDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/me", GetCurrentUser)
                .WithName("GetCurrentUser")
                .WithSummary("Lấy thông tin người dùng hiện tại")
                .WithDescription("Trả về thông tin chi tiết của người dùng đã xác thực hiện tại")
                .RequireAuthorization()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id}", GetUserById)
                .WithName("GetUserById")
                .WithSummary("Lấy thông tin người dùng theo ID")
                .WithDescription("Trả về thông tin chi tiết của người dùng dựa trên ID được cung cấp")
                .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetCurrentUser(ICurrentUserService currentUserService, IUserService userService)
        {
            var userId = currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await userService.GetUserByIdAsync(userId);

            if (result != null)
            {
                return Results.Ok(result);
            }

            return Results.NotFound("Không tìm thấy người dùng hiện tại.");
        }

        private static async Task<IResult> GetAllUsers([AsParameters] PaginationRequest request, IUserService userService)
        {
            var result = await userService.GetAllUsersAsync(request);

            if (result != null)
            {
                return Results.Ok(result);
            }

            return Results.NotFound("Không tìm thấy danh sách người dùng.");
        }

        private static async Task<IResult> GetUserById(Guid id, IUserService userService)
        {
            var result = await userService.GetUserByIdAsync(id);

            if (result != null)
            {
                return Results.Ok(result);
            }

            return Results.NotFound("Không tìm thấy người dùng.");
        }
    }
}
