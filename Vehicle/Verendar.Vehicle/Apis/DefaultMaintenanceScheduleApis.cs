using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

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
            group.MapGet("/{vehicleModelId:guid}/default-schedules", GetDefaultSchedulesByVehicleModelId)
                .WithName("GetDefaultMaintenanceSchedules")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy TẤT CẢ lịch bảo dưỡng mặc định của hãng cho mẫu xe";
                    operation.Description = "Trả về danh sách tất cả lịch bảo dưỡng cho mẫu xe này";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<DefaultMaintenanceScheduleResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<DefaultMaintenanceScheduleResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{vehicleModelId:guid}/part-categories/{partCategoryCode}/default-schedule", GetDefaultScheduleByPartCategory)
                .WithName("GetDefaultMaintenanceScheduleByPartCategory")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy lịch bảo dưỡng cho MỘT linh kiện cụ thể";
                    operation.Description = "Trả về lịch bảo dưỡng cho 1 linh kiện (engine_oil, oil_filter, etc.)";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<DefaultMaintenanceScheduleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<DefaultMaintenanceScheduleResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetDefaultSchedulesByVehicleModelId(
            Guid vehicleModelId,
            IDefaultMaintenanceScheduleService service,
            CancellationToken cancellationToken)
        {
            var result = await service.GetByVehicleModelIdAsync(vehicleModelId, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        }

        private static async Task<IResult> GetDefaultScheduleByPartCategory(
            Guid vehicleModelId,
            string partCategoryCode,
            IDefaultMaintenanceScheduleService service,
            CancellationToken cancellationToken)
        {
            var result = await service.GetByVehicleModelAndPartCategoryAsync(vehicleModelId, partCategoryCode, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        }
    }
}
