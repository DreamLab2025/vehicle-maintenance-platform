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
            .Produces<ApiResponse<List<ProvinceResponse>>>(200);

        group.MapGet("/provinces/{code}", GetProvinceByCode)
            .WithName("GetProvinceByCode")
            .Produces<ApiResponse<ProvinceResponse>>(200)
            .Produces<ApiResponse<ProvinceResponse>>(404);

        group.MapGet("/provinces/{code}/wards", GetWardsByProvince)
            .WithName("GetWardsByProvince")
            .Produces<ApiResponse<List<WardResponse>>>(200);

        group.MapGet("/wards/{code}", GetWardByCode)
            .WithName("GetWardByCode")
            .Produces<ApiResponse<WardResponse>>(200)
            .Produces<ApiResponse<WardResponse>>(404);

        group.MapGet("/administrative-units", GetAdministrativeUnits)
            .WithName("GetAdministrativeUnits")
            .Produces<ApiResponse<List<AdministrativeUnitResponse>>>(200);

        group.MapGet("/administrative-regions", GetAdministrativeRegions)
            .WithName("GetAdministrativeRegions")
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
