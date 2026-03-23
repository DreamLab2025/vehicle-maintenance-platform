using Verendar.Common.EndpointFilters;
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

            group.MapPost("/mechanic", CreateMechanic)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateMechanicRequest>())
                .WithName("CreateMechanicUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo tài khoản mechanic (internal — Garage service)";
                    return operation;
                })
                .Produces<CreateMechanicResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status400BadRequest);

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

        private static async Task<IResult> CreateMechanic(CreateMechanicRequest request, IUserService userService)
        {
            var result = await userService.CreateMechanicAsync(request);
            if (!result.IsSuccess)
            {
                return result.StatusCode == 409
                    ? Results.Conflict(new { error = result.Message })
                    : Results.Problem(result.Message, statusCode: 500);
            }

            return Results.Json(result.Data, statusCode: 201);
        }
    }

    public record UserEmailResponse(string Email);
}
