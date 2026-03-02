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
            group.MapGet("/vehicles/{userVehicleId:guid}", GetMaintenanceHistory)
                .WithName("GetMaintenanceHistory")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy lịch sử bảo dưỡng theo xe";
                    operation.Description = "Trả về danh sách phiếu bảo dưỡng của xe (có product hoặc không đều đồng bộ qua PartName/ItemCount).";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<IReadOnlyList<MaintenanceRecordSummaryDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{maintenanceRecordId:guid}", GetMaintenanceRecordDetail)
                .WithName("GetMaintenanceRecordDetail")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy chi tiết phiếu bảo dưỡng";
                    operation.Description = "Trả về chi tiết phiếu bảo dưỡng và danh sách phụ tùng; PartName đồng bộ (từ sản phẩm hoặc tên tùy chỉnh).";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<MaintenanceRecordDetailDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<MaintenanceRecordDetailDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

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

        private static async Task<IResult> GetMaintenanceHistory(
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await maintenanceRecordService.GetMaintenanceHistoryAsync(userId, userVehicleId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetMaintenanceRecordDetail(
            Guid maintenanceRecordId,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await maintenanceRecordService.GetMaintenanceRecordDetailAsync(userId, maintenanceRecordId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
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
