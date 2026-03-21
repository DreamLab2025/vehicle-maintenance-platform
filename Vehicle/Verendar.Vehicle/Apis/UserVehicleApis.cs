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
                .Produces<ApiResponse<List<PartSummary>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<PartSummary>>>(StatusCodes.Status404NotFound)
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
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UserVehicleRequest>())
                .WithName("CreateUserVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Thêm xe mới";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{userVehicleId:guid}", UpdateUserVehicle)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UserVehicleRequest>())
                .WithName("UpdateUserVehicle")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật thông tin xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<UserVehicleResponse>>(StatusCodes.Status409Conflict)
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
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{userVehicleId:guid}/streak", GetVehicleStreak)
                .WithName("GetVehicleStreak")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy chuỗi streak của xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<StreakResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{userVehicleId:guid}/apply-tracking", ApplyTrackingConfig)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ApplyTrackingConfigRequest>())
                .WithName("ApplyTrackingConfig")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Áp dụng cấu hình tracking từ AI cho một linh kiện";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<PartTrackingSummary>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartTrackingSummary>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<PartTrackingSummary>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{userVehicleId:guid}/parts/{partTrackingId:guid}/cycles", GetPartCycles)
                .WithName("GetPartCycles")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy toàn bộ tracking cycle của một phụ tùng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<TrackingCycleSummary>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<TrackingCycleSummary>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{userVehicleId:guid}/reminders", GetReminders)
                .WithName("GetReminders")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách nhắc bảo trì hiện tại (mỗi part category một reminder mới nhất)";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<ReminderDetailDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ReminderDetailDto>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> IsAllowedToCreateVehicle(ICurrentUserService currentUserService, IUserVehicleService userVehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await userVehicleService.IsAllowedToCreateVehicleAsync(userId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetVehicleStreak(ICurrentUserService currentUserService, IOdometerHistoryService odometerHistoryService, Guid userVehicleId)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await odometerHistoryService.GetVehicleStreakAsync(userId, userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetUserVehicles(ICurrentUserService currentUserService, [AsParameters] PaginationRequest paginationRequest, IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await vehicleService.GetUserVehiclesAsync(userId, paginationRequest);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetUserVehicleById(Guid userVehicleId, ICurrentUserService currentUserService, IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await vehicleService.GetUserVehicleByIdAsync(userId, userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetUserVehicleParts(Guid userVehicleId, ICurrentUserService currentUserService, IPartTrackingService trackingService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await trackingService.GetPartsByUserVehicleAsync(userId, userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateUserVehicle(UserVehicleRequest request, ICurrentUserService currentUserService, IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await vehicleService.CreateUserVehicleAsync(userId, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateUserVehicle(Guid userVehicleId, UserVehicleRequest request, ICurrentUserService currentUserService, IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await vehicleService.UpdateUserVehicleAsync(userId, userVehicleId, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteUserVehicle(Guid userVehicleId, ICurrentUserService currentUserService, IUserVehicleService vehicleService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await vehicleService.DeleteUserVehicleAsync(userId, userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetPartCycles(Guid userVehicleId, Guid partTrackingId, ICurrentUserService currentUserService, IPartTrackingService trackingService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await trackingService.GetCyclesForPartAsync(userId, userVehicleId, partTrackingId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetReminders(Guid userVehicleId, ICurrentUserService currentUserService, IMaintenanceReminderService reminderService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await reminderService.GetRemindersAsync(userId, userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> ApplyTrackingConfig(Guid userVehicleId, ApplyTrackingConfigRequest request, ICurrentUserService currentUserService, IPartTrackingService trackingService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await trackingService.ApplyTrackingConfigAsync(userId, userVehicleId, request);
            return result.ToHttpResult();
        }
    }
}
