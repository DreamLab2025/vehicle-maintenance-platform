namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Repositories.Interfaces;

public class ProvinceService(ILogger<ProvinceService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IProvinceService
{
    public async Task<ApiResponse<List<ProvinceResponse>>> GetAllProvincesAsync()
    {
        try
        {
            var cached = await cacheService.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll);
            if (cached != null)
                return ApiResponse<List<ProvinceResponse>>.SuccessResponse(cached, "Lấy danh sách tỉnh thành công");

            var provinces = await unitOfWork.Provinces.GetAllAsync();
            var response = provinces.Select(p => p.ToResponse()).ToList();

            await cacheService.SetAsync(CacheKeys.ProvincesAll, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<List<ProvinceResponse>>.SuccessResponse(response, "Lấy danh sách tỉnh thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllProvinces: failed");
            return ApiResponse<List<ProvinceResponse>>.FailureResponse("Lỗi khi lấy danh sách tỉnh");
        }
    }

    public async Task<ApiResponse<ProvinceResponse>> GetProvinceByCodeAsync(string code)
    {
        try
        {
            var cacheKey = CacheKeys.ProvinceByCode(code);
            var cached = await cacheService.GetAsync<ProvinceResponse>(cacheKey);
            if (cached != null)
                return ApiResponse<ProvinceResponse>.SuccessResponse(cached, "Lấy thông tin tỉnh thành công");

            var province = await unitOfWork.Provinces.GetByCodeAsync(code);
            if (province == null)
            {
                logger.LogWarning("GetProvinceByCode: not found {Code}", code);
                return ApiResponse<ProvinceResponse>.NotFoundResponse("Tỉnh không tồn tại");
            }

            var response = province.ToResponse();
            await cacheService.SetAsync(cacheKey, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<ProvinceResponse>.SuccessResponse(response, "Lấy thông tin tỉnh thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProvinceByCode: failed for {Code}", code);
            return ApiResponse<ProvinceResponse>.FailureResponse("Lỗi khi lấy thông tin tỉnh");
        }
    }

    public async Task<ApiResponse<List<WardResponse>>> GetWardsByProvinceAsync(string provinceCode)
    {
        try
        {
            var province = await unitOfWork.Provinces.GetByCodeAsync(provinceCode);
            if (province == null)
            {
                logger.LogWarning("GetWardsByProvince: province not found {ProvinceCode}", provinceCode);
                return ApiResponse<List<WardResponse>>.NotFoundResponse("Tỉnh không tồn tại");
            }

            var cacheKey = CacheKeys.WardsOfProvince(province.Code);
            var cached = await cacheService.GetAsync<List<WardResponse>>(cacheKey);
            if (cached != null)
                return ApiResponse<List<WardResponse>>.SuccessResponse(cached, "Lấy danh sách phường/xã thành công");

            var wards = await unitOfWork.Wards.GetByProvinceCodeAsync(province.Code);
            var response = wards.Select(w => w.ToResponse()).ToList();

            await cacheService.SetAsync(cacheKey, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<List<WardResponse>>.SuccessResponse(response, "Lấy danh sách phường/xã thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetWardsByProvince: failed for {ProvinceCode}", provinceCode);
            return ApiResponse<List<WardResponse>>.FailureResponse("Lỗi khi lấy danh sách phường/xã");
        }
    }

    public async Task<ApiResponse<ProvinceBoundaryResponse>> GetProvinceBoundaryAsync(string code)
    {
        try
        {
            var province = await unitOfWork.Provinces.GetByCodeAsync(code);
            if (province == null)
            {
                logger.LogWarning("GetProvinceBoundary: not found {Code}", code);
                return ApiResponse<ProvinceBoundaryResponse>.NotFoundResponse("Tỉnh không tồn tại");
            }

            var response = new ProvinceBoundaryResponse
            {
                Code = province.Code,
                Name = province.Name,
                BoundaryUrl = province.BoundaryUrl
            };
            return ApiResponse<ProvinceBoundaryResponse>.SuccessResponse(response, "Lấy boundary URL thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProvinceBoundary: failed for {Code}", code);
            return ApiResponse<ProvinceBoundaryResponse>.FailureResponse("Lỗi khi lấy boundary URL");
        }
    }
}
