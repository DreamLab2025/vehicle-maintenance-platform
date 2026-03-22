namespace Verendar.Vehicle.Apis
{
    public static class OdometerHistoryApis
    {
        public static IEndpointRouteBuilder MapOdometerHistoryApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/odometer-history")
                .MapOdometerHistoryRoutes()
                .WithTags("Odometer History Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapOdometerHistoryRoutes(this RouteGroupBuilder group)
        {
            group.MapPatch("/{userVehicleId:guid}", UpdateOdometer)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateOdometerRequest>())
                .WithName("UpdateOdometer")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật số km";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<UpdateOdometerResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UpdateOdometerResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<UpdateOdometerResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/", GetOdometerHistory)
                .WithName("GetOdometerHistory")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy lịch sử số km phân trang";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<OdometerHistoryItemDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<OdometerHistoryItemDto>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> UpdateOdometer(
            Guid userVehicleId,
            UpdateOdometerRequest request,
            ICurrentUserService currentUserService,
            IOdometerHistoryService odometerHistoryService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await odometerHistoryService.UpdateOdometerAsync(userId, userVehicleId, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetOdometerHistory(
            [AsParameters] OdometerHistoryQueryRequest query,
            ICurrentUserService currentUserService,
            IOdometerHistoryService odometerHistoryService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            query.Normalize();
            var result = await odometerHistoryService.GetOdometerHistoryPagedAsync(userId, query.UserVehicleId, query);
            return result.ToHttpResult();
        }
    }
}
