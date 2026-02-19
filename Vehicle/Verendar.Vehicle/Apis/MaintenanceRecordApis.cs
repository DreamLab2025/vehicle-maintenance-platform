using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Common.EndpointFilters;

namespace Verendar.Vehicle.Apis
{
    public static class MaintenanceRecordApis
    {
        public static IEndpointRouteBuilder MapMaintenanceRecordApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/maintenance-records")
                .MapMaintenanceRecordRoutes()
                .WithTags("Maintenance Record Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapMaintenanceRecordRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/vehicles/{userVehicleId:guid}", CreateMaintenanceRecord)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateMaintenanceRecordRequest>())
                .WithName("CreateMaintenanceRecord")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo phiếu bảo dưỡng (1 lần maintenance, nhiều phụ tùng thay thế)";
                    operation.Description = "Tạo một phiếu bảo dưỡng với nhiều item: cùng ngày/số km, mỗi item là một phụ tùng được thay thế và ghi nhận tracking tương ứng.";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<CreateMaintenanceRecordResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<CreateMaintenanceRecordResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> CreateMaintenanceRecord(
            Guid userVehicleId,
            CreateMaintenanceRecordRequest request,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await maintenanceRecordService.CreateMaintenanceRecordAsync(userId, userVehicleId, request);
            return result.IsSuccess ? Results.Created(string.Empty, result) : Results.BadRequest(result);
        }
    }
}
