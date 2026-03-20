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

            return group;
        }

        private static async Task<IResult> GetCurrentUser(ICurrentUserService currentUserService, IUserService userService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await userService.GetUserByIdAsync(userId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAllUsers([AsParameters] PaginationRequest paginationRequest, IUserService userService)
        {
            var result = await userService.GetAllUsersAsync(paginationRequest);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetUserById(Guid id, IUserService userService)
        {
            var result = await userService.GetUserByIdAsync(id);
            return result.ToHttpResult();
        }
    }
}
