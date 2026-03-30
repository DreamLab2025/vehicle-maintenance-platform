namespace Verendar.Location.Apis;

public static class LocationApis
{
    public static IEndpointRouteBuilder MapLocationApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/locations")
            .MapLocationRoutes()
            .WithTags("Location Api")
            .RequireRateLimiting("Fixed")
            .AddEndpointFilter(async (context, next) =>
            {
                var result = await next(context);
                context.HttpContext.Response.Headers.CacheControl = "public, max-age=86400";
                return result;
            });
        return builder;
    }

    public static RouteGroupBuilder MapLocationRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/provinces", GetAllProvinces)
            .WithName("GetAllProvinces")
            .WithOpenApi()
            .WithDescription("Lấy danh sách tất cả tỉnh/thành phố")
            .WithSummary("Danh sách tỉnh/thành phố")
            .Produces<ApiResponse<List<ProvinceResponse>>>(200);

        group.MapGet("/provinces/{code}", GetProvinceByCode)
            .WithName("GetProvinceByCode")
            .WithOpenApi()
            .WithDescription("Lấy thông tin chi tiết của một tỉnh/thành phố theo mã")
            .WithSummary("Chi tiết tỉnh/thành phố")
            .Produces<ApiResponse<ProvinceResponse>>(200)
            .Produces<ApiResponse<ProvinceResponse>>(404);

        group.MapGet("/provinces/{code}/boundary", GetProvinceBoundary)
            .WithName("GetProvinceBoundary")
            .WithOpenApi()
            .WithDescription("Lấy CloudFront URL chứa file GeoJSON boundary của tỉnh/thành phố")
            .WithSummary("Boundary URL tỉnh/thành phố")
            .Produces<ApiResponse<ProvinceBoundaryResponse>>(200)
            .Produces<ApiResponse<ProvinceBoundaryResponse>>(404);

        group.MapGet("/provinces/{code}/wards", GetWardsByProvince)
            .WithName("GetWardsByProvince")
            .WithOpenApi()
            .WithDescription("Lấy danh sách tất cả phường/xã thuộc một tỉnh/thành phố")
            .WithSummary("Danh sách phường/xã theo tỉnh")
            .Produces<ApiResponse<List<WardResponse>>>(200)
            .Produces<ApiResponse<List<WardResponse>>>(404);

        group.MapGet("/wards/{code}", GetWardByCode)
            .WithName("GetWardByCode")
            .WithOpenApi()
            .WithDescription("Lấy thông tin chi tiết của một phường/xã theo mã")
            .WithSummary("Chi tiết phường/xã")
            .Produces<ApiResponse<WardResponse>>(200)
            .Produces<ApiResponse<WardResponse>>(404);

        group.MapGet("/administrative-units", GetAdministrativeUnits)
            .WithName("GetAdministrativeUnits")
            .WithOpenApi()
            .WithDescription("Lấy danh sách nhãn loại đơn vị hành chính.")
            .WithSummary("Danh sách loại đơn vị hành chính")
            .Produces<ApiResponse<List<AdministrativeUnitResponse>>>(200);

        group.MapGet("/administrative-regions", GetAdministrativeRegions)
            .WithName("GetAdministrativeRegions")
            .WithOpenApi()
            .WithDescription("Lấy danh sách các vùng miền hành chính (Miền Bắc, Miền Trung, Miền Nam, v.v.)")
            .WithSummary("Danh sách vùng miền hành chính")
            .Produces<ApiResponse<List<AdministrativeRegionResponse>>>(200);

        return group;
    }

    private static async Task<IResult> GetAllProvinces(IProvinceService service)
    {
        var result = await service.GetAllProvincesAsync();
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetProvinceByCode(string code, IProvinceService service)
    {
        var result = await service.GetProvinceByCodeAsync(code);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetProvinceBoundary(string code, IProvinceService service)
    {
        var result = await service.GetProvinceBoundaryAsync(code);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetWardsByProvince(string code, IProvinceService service)
    {
        var result = await service.GetWardsByProvinceAsync(code);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetWardByCode(string code, IWardService service)
    {
        var result = await service.GetWardByCodeAsync(code);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAdministrativeUnits(IAdministrativeUnitService service)
    {
        var result = await service.GetAllAsync();
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAdministrativeRegions(IAdministrativeRegionService service)
    {
        var result = await service.GetAllAsync();
        return result.ToHttpResult();
    }
}
