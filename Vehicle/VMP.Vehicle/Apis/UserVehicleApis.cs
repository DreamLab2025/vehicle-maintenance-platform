using System.Security.Claims;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Services.Interfaces;

namespace VMP.Vehicle.Apis
{
    public static class UserVehicleApis
    {
        public static IEndpointRouteBuilder MapUserVehicleApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/user-vehicles")
                .MapUserVehicleRoutes()
                .WithTags("User Vehicle Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapUserVehicleRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetUserVehicles)
                .WithName("GetUserVehicles")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách xe của người dùng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<UserVehicleResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<UserVehicleResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetUserVehicleById)
                .WithName("GetUserVehicleById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin chi tiết xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleDetailResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleDetailResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateUserVehicle)
                .WithName("CreateUserVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Thêm xe mới";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateUserVehicle)
                .WithName("UpdateUserVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật thông tin xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPatch("/{id:guid}/odometer", UpdateOdometer)
                .WithName("UpdateOdometer")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật số km";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteUserVehicle)
                .WithName("DeleteUserVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private static async Task<IResult> GetUserVehicles(
            ClaimsPrincipal user,
            [AsParameters] PaginationRequest paginationRequest,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.GetUserVehiclesAsync(userId, paginationRequest);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetUserVehicleById(
            Guid id,
            ClaimsPrincipal user,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.GetUserVehicleByIdAsync(userId, id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateUserVehicle(
            UserVehicleRequest request,
            ClaimsPrincipal user,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.CreateUserVehicleAsync(userId, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateUserVehicle(
            Guid id,
            UserVehicleRequest request,
            ClaimsPrincipal user,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.UpdateUserVehicleAsync(userId, id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateOdometer(
            Guid id,
            UpdateOdometerRequest request,
            ClaimsPrincipal user,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.UpdateOdometerAsync(userId, id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteUserVehicle(
            Guid id,
            ClaimsPrincipal user,
            IUserVehicleService vehicleService)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.DeleteUserVehicleAsync(userId, id);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
