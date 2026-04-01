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
            {
                logger.LogWarning("GetWardByCode: not found {Code}", code);
                return ApiResponse<WardResponse>.NotFoundResponse("Phường/xã không tồn tại");
            }

            var response = ward.ToResponse();
            await cacheService.SetAsync(cacheKey, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<WardResponse>.SuccessResponse(response, "Lấy thông tin phường/xã thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetWardByCode: failed for {Code}", code);
            return ApiResponse<WardResponse>.FailureResponse("Lỗi khi lấy thông tin phường/xã");
        }
    }

    public async Task<ApiResponse<WardBoundaryResponse>> GetWardBoundaryAsync(string code)
    {
        try
        {
            var ward = await unitOfWork.Wards.GetByCodeAsync(code);
            if (ward == null)
            {
                logger.LogWarning("GetWardBoundary: not found {Code}", code);
                return ApiResponse<WardBoundaryResponse>.NotFoundResponse("Phường/xã không tồn tại");
            }

            var response = new WardBoundaryResponse
            {
                Code = ward.Code,
                Name = ward.Name,
                BoundaryUrl = ward.BoundaryUrl,
                BoundaryShardMatchValue = ward.Code.PadLeft(5, '0')
            };
            return ApiResponse<WardBoundaryResponse>.SuccessResponse(response, "Lấy boundary URL thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetWardBoundary: failed for {Code}", code);
            return ApiResponse<WardBoundaryResponse>.FailureResponse("Lỗi khi lấy boundary URL");
        }
    }
}
