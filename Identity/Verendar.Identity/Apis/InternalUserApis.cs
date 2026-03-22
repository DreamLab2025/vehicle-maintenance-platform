using Verendar.Common.Shared;

namespace Verendar.Identity.Apis
{
    public static class InternalUserApis
    {
        public static IEndpointRouteBuilder MapInternalUserApi(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/internal/users")
                .WithTags("Internal User Api")
                .RequireAuthorization(policy => policy.RequireRole("Service"));

            group.MapGet("/{id:guid}/email", GetUserEmailById)
                .WithName("GetUserEmailById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy email người dùng theo ID (internal)";
                    return operation;
                })
                .Produces<UserEmailResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        private static async Task<IResult> GetUserEmailById(Guid id, IUserService userService)
        {
            var result = await userService.GetUserByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
                return Results.NotFound();

            var email = result.Data.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Results.NotFound();

            return Results.Ok(ApiResponse<UserEmailResponse>.SuccessResponse(new UserEmailResponse(email)));
        }
    }

    public record UserEmailResponse(string Email);
}
