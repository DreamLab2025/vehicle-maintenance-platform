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

        return group;
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
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await garageService.CreateGarageAsync(userId, request);
        return result.ToHttpResult();
    }
}
