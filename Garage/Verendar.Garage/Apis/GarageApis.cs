using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;


namespace Verendar.Garage.Apis;

public static class GarageApis
{
    public static IEndpointRouteBuilder MapGarageApis(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/v1/garages")
            .MapGarageRoutes()
            .WithTags("Garage");

        return app;
    }

    private static RouteGroupBuilder MapGarageRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/business-lookup/{taxCode}", LookupBusiness)
            .WithSummary("Tra cứu thông tin doanh nghiệp theo MST để điền form tự động");

        return group;
    }

    private static async Task<IResult> LookupBusiness(
        string taxCode,
        IVietQRClient vietQRClient,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(taxCode) || taxCode.Length > 20)
            return Results.UnprocessableEntity(
                ApiResponse<object>.FailureResponse("MST không hợp lệ (tối đa 20 ký tự)."));

        var info = await vietQRClient.LookupBusinessAsync(taxCode.Trim(), ct);

        if (info is null)
            return Results.NotFound(
                ApiResponse<object>.NotFoundResponse($"Không tìm thấy doanh nghiệp với MST '{taxCode}'."));

        return Results.Ok(ApiResponse<BusinessInfoDto>.SuccessResponse(info));
    }
}
