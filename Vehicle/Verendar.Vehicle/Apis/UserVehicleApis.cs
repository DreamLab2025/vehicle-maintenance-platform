using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Apis
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

            group.MapGet("/{userVehicleId:guid}", GetUserVehicleById)
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

            group.MapGet("/{userVehicleId:guid}/parts", GetUserVehicleParts)
                .WithName("GetUserVehicleParts")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách phụ tùng của xe người dùng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<UserVehiclePartSummary>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<UserVehiclePartSummary>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/is-allowed-create", IsAllowedToCreateVehicle)
                .WithName("IsAllowedToCreateVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Kiểm tra xem người dùng có được tạo xe mới không";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<IsAllowedToCreateVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<IsAllowedToCreateVehicleResponse>>(StatusCodes.Status400BadRequest)
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

            group.MapPut("/{userVehicleId:guid}", UpdateUserVehicle)
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

            group.MapPatch("/{userVehicleId:guid}/odometer", UpdateOdometer)
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

            group.MapDelete("/{userVehicleId:guid}", DeleteUserVehicle)
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

            group.MapGet("/{userVehicleId:guid}/streak", GetVehicleStreak)
                .WithName("GetVehicleStreak")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy chuỗi streak của xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<VehicleStreakResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{userVehicleId:guid}/apply-tracking", ApplyTrackingConfig)
                .WithName("ApplyTrackingConfig")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Áp dụng cấu hình tracking từ AI cho một linh kiện";
                    operation.Description = "Sau khi AI phân tích questionnaire, frontend gọi endpoint này " +
                                          "để áp dụng khuyến nghị của AI vào VehiclePartTracking. " +
                                          "Endpoint này cập nhật LastReplacement và PredictedNext từ AI.";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<VehiclePartTrackingSummary>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VehiclePartTrackingSummary>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> IsAllowedToCreateVehicle(
            ICurrentUserService currentUserService,
            IUserVehicleService userVehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }
            var result = await userVehicleService.IsAllowedToCreateVehicleAsync(userId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetVehicleStreak(
            ICurrentUserService currentUserService,
            IUserVehicleService userVehicleService,
            Guid userVehicleId)
        {
            var result = await userVehicleService.GetVehicleStreakAsync(userVehicleId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetUserVehicles(
            ICurrentUserService currentUserService,
            [AsParameters] PaginationRequest paginationRequest,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.GetUserVehiclesAsync(userId, paginationRequest);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetUserVehicleById(
            Guid id,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.GetUserVehicleByIdAsync(userId, id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetUserVehicleParts(
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.GetPartsByUserVehicleAsync(userId, userVehicleId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateUserVehicle(
            UserVehicleRequest request,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
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
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
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
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.UpdateOdometerAsync(userId, id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteUserVehicle(
            Guid id,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.DeleteUserVehicleAsync(userId, id);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ApplyTrackingConfig(
            Guid id,
            ApplyTrackingConfigRequest request,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await vehicleService.ApplyTrackingConfigAsync(userId, id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
