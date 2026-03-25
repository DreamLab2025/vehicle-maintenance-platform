using Verendar.Ai.Application.Dtos.OdometerScan;

namespace Verendar.Ai.Apis
{
    public static class OdometerScanApis
    {
        public static IEndpointRouteBuilder MapOdometerScanApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/ai/odometer-scans")
                .MapOdometerScanRoutes()
                .WithTags("Odometer Scan Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapOdometerScanRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/", ScanOdometer)
                .WithName("ScanOdometer")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đọc số km từ ảnh đồng hồ xe";
                    operation.Description = "Upload ảnh chụp đồng hồ công-tơ-mét, AI sẽ nhận diện và trả về số km. User cần xác nhận trước khi lưu.";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<OdometerScanResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<OdometerScanResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> ScanOdometer(
            OdometerScanRequest request,
            IOdometerScanService odometerScanService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await odometerScanService.ScanOdometerAsync(userId, request, cancellationToken);
            return result.ToHttpResult();
        }
    }
}
