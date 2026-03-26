using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageApis
{
    public static IEndpointRouteBuilder MapGarageApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages")
            .MapGarageRoutes()
            .WithTags("Garage Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetGarages)
            .WithName("GetGarages")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách garage có phân trang, lọc theo trạng thái";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<List<GarageResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetMyGarage)
            .WithName("GetMyGarage")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xem thông tin garage của tôi kèm danh sách chi nhánh";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageDetailResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetGarageById)
            .WithName("GetGarageById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xem chi tiết garage và danh sách chi nhánh";
                return operation;
            })
            .Produces<ApiResponse<GarageDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageDetailResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("/business-lookup/{taxCode}", LookupBusiness)
            .WithName("LookupBusiness")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tra cứu thông tin doanh nghiệp theo MST để điền form tự động";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<BusinessInfoDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BusinessInfoDto>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateGarage)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageRequest>())
            .WithName("CreateGarage")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Đăng ký garage mới (mỗi tài khoản chỉ được một garage)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateGarageStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageStatusRequest>())
            .WithName("UpdateGarageStatus")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Admin duyệt / từ chối / tạm khóa garage";
                return operation;
            })
            .RequireAuthorization("Admin")
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateGarageInfo)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageRequest>())
            .WithName("UpdateGarageInfo")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Chủ garage chỉnh sửa thông tin (chỉ khi Pending hoặc Rejected)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/resubmit", ResubmitGarage)
            .WithName("ResubmitGarage")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Chủ garage nộp lại hồ sơ sau khi bị từ chối (Rejected → Pending)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetGarages(
        [AsParameters] GarageFilterRequest request,
        IGarageService garageService)
    {
        var result = await garageService.GetGaragesAsync(request);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMyGarage(
        ICurrentUserService currentUserService,
        IGarageService garageService,
        CancellationToken ct)
    {
        var result = await garageService.GetMyGarageAsync(currentUserService.UserId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetGarageById(
        Guid id,
        IGarageService garageService,
        CancellationToken ct)
    {
        var result = await garageService.GetGarageByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> LookupBusiness(
        string taxCode,
        IBusinessLookupService businessLookupService,
        CancellationToken ct)
    {
        var info = await businessLookupService.LookupBusinessAsync(taxCode.Trim(), ct);

        if (info is null)
            return ApiResponse<BusinessInfoDto>.NotFoundResponse($"Không tìm thấy doanh nghiệp với MST '{taxCode}'.").ToHttpResult();

        return ApiResponse<BusinessInfoDto>.SuccessResponse(info).ToHttpResult();
    }

    private static async Task<IResult> CreateGarage(
        GarageRequest request,
        ICurrentUserService currentUserService,
        IGarageService garageService)
    {
        var result = await garageService.CreateGarageAsync(currentUserService.UserId, request);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateGarageStatus(
        Guid id,
        UpdateGarageStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageService garageService,
        CancellationToken ct)
    {
        var result = await garageService.UpdateGarageStatusAsync(id, request, currentUserService.UserId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateGarageInfo(
        Guid id,
        GarageRequest request,
        ICurrentUserService currentUserService,
        IGarageService garageService,
        CancellationToken ct)
    {
        var result = await garageService.UpdateGarageInfoAsync(id, currentUserService.UserId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ResubmitGarage(
        Guid id,
        ICurrentUserService currentUserService,
        IGarageService garageService,
        CancellationToken ct)
    {
        var result = await garageService.ResubmitGarageAsync(id, currentUserService.UserId, ct);
        return result.ToHttpResult();
    }
}
