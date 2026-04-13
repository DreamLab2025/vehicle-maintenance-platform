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
            group.MapGet("/", GetMaintenanceHistory)
                .WithName("GetMaintenanceHistory")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy lịch sử bảo dưỡng theo xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<IReadOnlyList<RecordSummaryDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{maintenanceRecordId:guid}", GetMaintenanceRecordDetail)
                .WithName("GetMaintenanceRecordDetail")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy chi tiết phiếu bảo dưỡng";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<RecordDetailDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<RecordDetailDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/manual", CreateManualMaintenanceRecord)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateManualRecordRequest>())
                .WithName("CreateManualMaintenanceRecord")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo phiếu bảo dưỡng thủ công (không qua proposal)";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<CreateRecordResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<CreateRecordResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/export", ExportMaintenance)
                .WithName("ExportMaintenanceRecords")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xuất lịch sử bảo dưỡng ra file PDF hoặc CSV";
                    return operation;
                })
                .RequireAuthorization()
                .Produces(StatusCodes.Status200OK)
                .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetMaintenanceHistory(
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService)
        {
            var result = await maintenanceRecordService.GetMaintenanceHistoryAsync(
                currentUserService.UserId,
                userVehicleId);
            return result.ToHttpResult();
        }


        private static async Task<IResult> GetMaintenanceRecordDetail(
            Guid maintenanceRecordId,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService)
        {
            var result = await maintenanceRecordService.GetMaintenanceRecordDetailAsync(
                currentUserService.UserId,
                maintenanceRecordId);
            return result.ToHttpResult();
        }


        private static async Task<IResult> CreateManualMaintenanceRecord(
            CreateManualRecordRequest request,
            ICurrentUserService currentUserService,
            IMaintenanceRecordService maintenanceRecordService,
            CancellationToken cancellationToken)
        {
            var result = await maintenanceRecordService.CreateManualMaintenanceRecordAsync(
                currentUserService.UserId,
                request,
                cancellationToken);
            return result.ToHttpResult();
        }

        private static async Task<IResult> ExportMaintenance(
            [AsParameters] ExportMaintenanceQueryRequest query,
            ICurrentUserService currentUserService,
            IMaintenanceExportService maintenanceExportService,
            CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<ExportFormat>(query.Format, ignoreCase: true, out var format))
                return Results.BadRequest(ApiResponse<object>.FailureResponse("Định dạng không hợp lệ. Sử dụng 'pdf' hoặc 'csv'."));

            var columns = string.IsNullOrWhiteSpace(query.Columns)
                ? null
                : query.Columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var request = new ExportMaintenanceRequest
            {
                UserVehicleId = query.UserVehicleId,
                Format = format,
                From = query.From,
                To = query.To,
                Columns = columns
            };

            var result = await maintenanceExportService.ExportAsync(currentUserService.UserId, request, cancellationToken);
            if (!result.IsSuccess)
                return result.ToHttpResult();

            var (data, contentType, fileName) = result.Data!;
            return Results.File(data, contentType, fileName);
        }
    }
}
