namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Domain.Repositories.Interfaces;

public class ProvinceService(ILogger<ProvinceService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IProvinceService
{
    public async Task<ApiResponse<List<ProvinceResponse>>> GetAllProvincesAsync()
    {
        try
        {
            const string cacheKey = "location:provinces:all";
            var cached = await cacheService.GetAsync<List<ProvinceResponse>>(cacheKey);
            if (cached != null)
                return ApiResponse<List<ProvinceResponse>>.SuccessResponse(cached, "Lấy danh sách tỉnh thành công");

            var provinces = await unitOfWork.Provinces.GetAllAsync();
            var response = provinces.Select(p => p.ToResponse()).ToList();

            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return ApiResponse<List<ProvinceResponse>>.SuccessResponse(response, "Lấy danh sách tỉnh thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all provinces");
            return ApiResponse<List<ProvinceResponse>>.FailureResponse("Lỗi khi lấy danh sách tỉnh");
        }
    }

    public async Task<ApiResponse<ProvinceResponse>> GetProvinceByCodeAsync(string code)
    {
        try
        {
            var cacheKey = $"location:provinces:{code}";
            var cached = await cacheService.GetAsync<ProvinceResponse>(cacheKey);
            if (cached != null)
                return ApiResponse<ProvinceResponse>.SuccessResponse(cached, "Lấy thông tin tỉnh thành công");

            var province = await unitOfWork.Provinces.GetByCodeAsync(code);
            if (province == null)
                return ApiResponse<ProvinceResponse>.NotFoundResponse("Tỉnh không tồn tại");

            var response = province.ToResponse();
            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return ApiResponse<ProvinceResponse>.SuccessResponse(response, "Lấy thông tin tỉnh thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting province by code: {Code}", code);
            return ApiResponse<ProvinceResponse>.FailureResponse("Lỗi khi lấy thông tin tỉnh");
        }
    }

    public async Task<ApiResponse<List<WardResponse>>> GetWardsByProvinceAsync(string provinceCode)
    {
        try
        {
            var cacheKey = $"location:provinces:{provinceCode}:wards";
            var cached = await cacheService.GetAsync<List<WardResponse>>(cacheKey);
            if (cached != null)
                return ApiResponse<List<WardResponse>>.SuccessResponse(cached, "Lấy danh sách phường/xã thành công");

            var wards = await unitOfWork.Wards.GetByProvinceCodeAsync(provinceCode);
            var response = wards.Select(w => w.ToResponse()).ToList();

            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return ApiResponse<List<WardResponse>>.SuccessResponse(response, "Lấy danh sách phường/xã thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting wards for province: {ProvinceCode}", provinceCode);
            return ApiResponse<List<WardResponse>>.FailureResponse("Lỗi khi lấy danh sách phường/xã");
        }
    }
}
