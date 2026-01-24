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
                    operation.Summary = "Lấy lịch bảo dưỡng mặc định của hãng cho mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<DefaultMaintenanceScheduleResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<DefaultMaintenanceScheduleResponse>>>(StatusCodes.Status404NotFound)
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
    }
}
