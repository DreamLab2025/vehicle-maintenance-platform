using System.Security.Cryptography;
using System.Text;
using Verendar.Common.Caching;
using Verendar.Location.Application.Dtos;
using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Application.Shared.Const;

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

        group.MapGet("/search", SearchPlaces)
            .WithName("SearchPlaces")
            .WithOpenApi()
            .WithDescription("Tìm kiếm địa điểm theo từ khóa, trả về danh sách gợi ý (tương tự Google Maps autocomplete)")
            .WithSummary("Gợi ý địa điểm")
            .Produces<ApiResponse<List<PlaceSuggestionResponse>>>(200)
            .Produces<ApiResponse<List<PlaceSuggestionResponse>>>(400);

        group.MapGet("/search/details/{placeId}", GetPlaceDetails)
            .WithName("GetPlaceDetails")
            .WithOpenApi()
            .WithDescription("Lấy tọa độ và địa chỉ đầy đủ của một địa điểm theo placeId")
            .WithSummary("Chi tiết địa điểm")
            .Produces<ApiResponse<PlaceDetailResponse>>(200)
            .Produces<ApiResponse<PlaceDetailResponse>>(400)
            .Produces<ApiResponse<PlaceDetailResponse>>(404);

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

    private static async Task<IResult> SearchPlaces(
        string? q,
        string? provinceCode,
        string? wardCode,
        string? sessionToken,
        IPlaceSearchService placeSearchService,
        IProvinceService provinceService,
        IWardService wardService,
        ICacheService cacheService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Results.BadRequest(ApiResponse<List<PlaceSuggestionResponse>>.FailureResponse("Từ khóa tìm kiếm phải có ít nhất 2 ký tự"));

        var normalizedQuery = q.Trim().ToLowerInvariant();

        string? provinceName = null;
        if (!string.IsNullOrWhiteSpace(provinceCode))
        {
            var province = await provinceService.GetProvinceByCodeAsync(provinceCode.Trim());
            if (province.IsSuccess && province.Data is not null)
                provinceName = province.Data.Name;
        }

        string? wardName = null;
        if (!string.IsNullOrWhiteSpace(wardCode))
        {
            var ward = await wardService.GetWardByCodeAsync(wardCode.Trim());
            if (ward.IsSuccess && ward.Data is not null)
                wardName = ward.Data.Name;
        }

        var cacheKey = CacheKeys.PlaceSearch(ComputeHash($"{normalizedQuery}:{provinceName}:{wardName}"));
        var cached = await cacheService.GetAsync<List<PlaceSuggestionResponse>>(cacheKey);
        if (cached is not null)
            return Results.Ok(ApiResponse<List<PlaceSuggestionResponse>>.SuccessResponse(cached, "Tìm kiếm địa điểm thành công"));

        var suggestions = await placeSearchService.SearchAsync(normalizedQuery, provinceName, wardName, sessionToken, ct);
        var response = suggestions.Select(s => new PlaceSuggestionResponse
        {
            PlaceId = s.PlaceId,
            Description = s.Description,
            MainText = s.MainText,
            SecondaryText = s.SecondaryText
        }).ToList();

        await cacheService.SetAsync(cacheKey, response, CacheKeys.PlaceSearchCacheDuration);
        return Results.Ok(ApiResponse<List<PlaceSuggestionResponse>>.SuccessResponse(response, "Tìm kiếm địa điểm thành công"));
    }

    private static async Task<IResult> GetPlaceDetails(
        string placeId,
        string? sessionToken,
        IPlaceSearchService placeSearchService,
        ICacheService cacheService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(placeId))
            return Results.BadRequest(ApiResponse<PlaceDetailResponse>.FailureResponse("placeId không được để trống"));

        var cacheKey = CacheKeys.PlaceDetail(placeId);
        var cached = await cacheService.GetAsync<PlaceDetailResponse>(cacheKey);
        if (cached is not null)
            return Results.Ok(ApiResponse<PlaceDetailResponse>.SuccessResponse(cached, "Lấy thông tin địa điểm thành công"));

        var detail = await placeSearchService.GetDetailsAsync(placeId, sessionToken, ct);
        if (detail is null)
            return Results.NotFound(ApiResponse<PlaceDetailResponse>.NotFoundResponse("Không tìm thấy địa điểm"));

        var response = new PlaceDetailResponse
        {
            PlaceId = detail.PlaceId,
            FormattedAddress = detail.FormattedAddress,
            Latitude = detail.Latitude,
            Longitude = detail.Longitude
        };

        await cacheService.SetAsync(cacheKey, response, CacheKeys.PlaceDetailCacheDuration);
        return Results.Ok(ApiResponse<PlaceDetailResponse>.SuccessResponse(response, "Lấy thông tin địa điểm thành công"));
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes)[..16];
    }
}
