namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Repositories.Interfaces;

public class WardService(ILogger<WardService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IWardService
{
    public async Task<ApiResponse<WardResponse>> GetWardByCodeAsync(string code)
    {
        try
        {
            var cacheKey = CacheKeys.WardByCode(code);
            var cached = await cacheService.GetAsync<WardResponse>(cacheKey);
            if (cached != null)
                return ApiResponse<WardResponse>.SuccessResponse(cached, "Lấy thông tin phường/xã thành công");

            var ward = await unitOfWork.Wards.GetByCodeAsync(code);
            if (ward == null)
                return ApiResponse<WardResponse>.NotFoundResponse("Phường/xã không tồn tại");

            var response = ward.ToResponse();
            await cacheService.SetAsync(cacheKey, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<WardResponse>.SuccessResponse(response, "Lấy thông tin phường/xã thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ward by code: {Code}", code);
            return ApiResponse<WardResponse>.FailureResponse("Lỗi khi lấy thông tin phường/xã");
        }
    }
}
