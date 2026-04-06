namespace Verendar.Vehicle.Apis
{
    public static class InternalVehicleApis
    {
        public static IEndpointRouteBuilder MapInternalVehicleApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/internal/vehicles")
                .WithTags("Internal Vehicle Api")
                .RequireAuthorization()
                .MapInternalVehicleRoutes();

            return builder;
        }

        private static RouteGroupBuilder MapInternalVehicleRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/user-vehicles/{userVehicleId:guid}", GetUserVehicleById)
                .WithName("InternalGetUserVehicleById");

            group.MapGet(
                    "/models/{vehicleModelId:guid}/part-categories/{partCategorySlug}/default-schedule",
                    GetDefaultScheduleForModelPartCategory)
                .WithName("InternalGetDefaultScheduleByModelPartCategory");

            group.MapGet("/user-vehicles/{userVehicleId:guid}/odometer-summary", GetOdometerSummary)
                .WithName("InternalGetOdometerSummary");

            group.MapGet("/user-vehicles/{userVehicleId:guid}/baseline-parts", GetBaselineParts)
                .WithName("InternalGetBaselineParts");

            group.MapPost("/user-vehicles/{vehicleId:guid}/apply-tracking", InternalApplyTracking)
                .WithName("InternalApplyTracking");

            return group;
        }

        private static async Task<IResult> GetUserVehicleById(
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IUserVehicleService vehicleService)
        {
            var result = await vehicleService.GetUserVehicleByIdAsync(
                currentUserService.UserId,
                userVehicleId);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetDefaultScheduleForModelPartCategory(
            Guid vehicleModelId,
            string partCategorySlug,
            IDefaultScheduleService scheduleService,
            CancellationToken cancellationToken)
        {
            var result = await scheduleService.GetDefaultScheduleByModelAndPartCategorySlugAsync(
                vehicleModelId,
                partCategorySlug,
                cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetOdometerSummary(
            Guid userVehicleId,
            IOdometerHistoryService odometerHistoryService,
            CancellationToken cancellationToken)
        {
            var result = await odometerHistoryService.GetSummaryAsync(userVehicleId, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetBaselineParts(
            Guid userVehicleId,
            IPartTrackingService partTrackingService,
            CancellationToken cancellationToken)
        {
            var result = await partTrackingService.GetBaselinePartsAsync(userVehicleId, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> InternalApplyTracking(
            Guid vehicleId,
            Guid userId,
            ApplyTrackingConfigRequest request,
            IPartTrackingService partTrackingService)
        {
            var result = await partTrackingService.ApplyTrackingConfigAsync(userId, vehicleId, request);
            return result.ToHttpResult();
        }
    }
}
