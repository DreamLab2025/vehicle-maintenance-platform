namespace Verendar.Vehicle.Apis
{
    public static class DefaultMaintenanceScheduleApis
    {
        public static IEndpointRouteBuilder MapDefaultMaintenanceScheduleApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/vehicle-models")
                .MapDefaultMaintenanceScheduleRoutes()
                .WithTags("Default Maintenance Schedule Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapDefaultMaintenanceScheduleRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/{vehicleModelId:guid}/part-categories", GetPartCategoriesByVehicleModel)
                .WithName("GetPartCategoriesByVehicleModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh mục linh kiện áp dụng cho mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartCategoryResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<PartCategoryResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{vehicleModelId:guid}/part-categories/{partCategoryCode}/default-schedule", GetDefaultScheduleByPartCategory)
                .WithName("GetDefaultMaintenanceScheduleByPartCategory")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy lịch bảo dưỡng cho MỘT linh kiện cụ thể";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<DefaultMaintenanceScheduleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<DefaultMaintenanceScheduleResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetPartCategoriesByVehicleModel(
            Guid vehicleModelId,
            IDefaultMaintenanceScheduleService service,
            CancellationToken cancellationToken)
        {
            var result = await service.GetPartCategoriesByVehicleModelAsync(vehicleModelId, cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetDefaultScheduleByPartCategory(
            Guid vehicleModelId,
            string partCategoryCode,
            IDefaultMaintenanceScheduleService service,
            CancellationToken cancellationToken)
        {
            var result = await service.GetByVehicleModelAndPartCategoryAsync(vehicleModelId, partCategoryCode, cancellationToken);
            return result.ToHttpResult();
        }
    }
}
