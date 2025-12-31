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
                .WithSummary("L?y danh sách xe c?a ng??i dùng")
                .WithDescription("Tr? v? danh sách t?t c? xe c?a ng??i dùng hi?n t?i")
                .RequireAuthorization()
                .Produces<ApiResponse<List<UserVehicleResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<UserVehicleResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetUserVehicleById)
                .WithName("GetUserVehicleById")
                .WithSummary("L?y thông tin chi ti?t xe")
                .WithDescription("Tr? v? thông tin chi ti?t c?a m?t xe")
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleDetailResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleDetailResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateUserVehicle)
                .WithName("CreateUserVehicle")
                .WithSummary("Thêm xe m?i")
                .WithDescription("Thêm m?t xe m?i vào danh sách c?a ng??i dùng")
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateUserVehicle)
                .WithName("UpdateUserVehicle")
                .WithSummary("C?p nh?t thông tin xe")
                .WithDescription("C?p nh?t thông tin c?a m?t xe")
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPatch("/{id:guid}/odometer", UpdateOdometer)
                .WithName("UpdateOdometer")
                .WithSummary("C?p nh?t s? km")
                .WithDescription("C?p nh?t s? km hi?n t?i c?a xe")
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteUserVehicle)
                .WithName("DeleteUserVehicle")
                .WithSummary("Xóa xe")
                .WithDescription("Xóa m?t xe kh?i danh sách")
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
