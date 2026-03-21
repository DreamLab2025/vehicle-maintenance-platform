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

            return group;
        }

        private static async Task<IResult> GetPartCategoriesByVehicleModel(
            Guid vehicleModelId,
            IDefaultScheduleService service,
            CancellationToken cancellationToken)
        {
            var result = await service.GetPartCategoriesByVehicleModelAsync(vehicleModelId, cancellationToken);
            return result.ToHttpResult();
        }

    }
}
