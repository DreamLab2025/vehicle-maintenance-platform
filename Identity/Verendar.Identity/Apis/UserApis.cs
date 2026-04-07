using Microsoft.AspNetCore.Mvc;
using Verendar.Common.EndpointFilters;

namespace Verendar.Identity.Apis
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
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả người dùng (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<List<UserDto>>>(StatusCodes.Status200OK);

            group.MapGet("/me", GetCurrentUser)
                .WithName("GetCurrentUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin người dùng hiện tại";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetUserById)
                .WithName("GetUserById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin người dùng theo ID (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateUser)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UserCreateRequest>())
                .WithName("CreateUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo người dùng (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status409Conflict);

            group.MapPut("/{id:guid}", UpdateUser)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UserUpdateRequest>())
                .WithName("UpdateUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật người dùng (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}", DeleteUser)
                .WithName("DeleteUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa mềm người dùng (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> GetCurrentUser(ICurrentUserService currentUserService, IUserService userService)
        {
            var result = await userService.GetUserByIdAsync(currentUserService.UserId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAllUsers([AsParameters] UserFilterRequest request, IUserService userService)
        {
            var result = await userService.GetAllUsersAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetUserById(Guid id, IUserService userService)
        {
            var result = await userService.GetUserByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateUser(
            [FromBody] UserCreateRequest request,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            var result = await userService.CreateUserAsync(request, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateUser(
            Guid id,
            [FromBody] UserUpdateRequest request,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            var result = await userService.UpdateUserAsync(id, request, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteUser(
            Guid id,
            ICurrentUserService currentUserService,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            var result = await userService.DeleteUserAsync(id, currentUserService.UserId, cancellationToken);
            return result.ToHttpResult();
        }
    }
}
