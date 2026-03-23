namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Repositories.Interfaces;

public class AdministrativeUnitService(ILogger<AdministrativeUnitService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IAdministrativeUnitService
{
    public async Task<ApiResponse<List<AdministrativeUnitResponse>>> GetAllAsync()
    {
        try
        {
            var cached = await cacheService.GetAsync<List<AdministrativeUnitResponse>>(CacheKeys.AdministrativeUnitsAll);
            if (cached != null)
                return ApiResponse<List<AdministrativeUnitResponse>>.SuccessResponse(cached, "Lấy danh sách loại đơn vị thành công");

            var units = await unitOfWork.AdministrativeUnits.GetAllAsync();
            var response = units.Select(u => u.ToResponse()).ToList();

            await cacheService.SetAsync(CacheKeys.AdministrativeUnitsAll, response, CacheKeys.DefaultCacheDuration);
            return ApiResponse<List<AdministrativeUnitResponse>>.SuccessResponse(response, "Lấy danh sách loại đơn vị thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAdministrativeUnits: failed");
            return ApiResponse<List<AdministrativeUnitResponse>>.FailureResponse("Lỗi khi lấy danh sách loại đơn vị");
        }
    }
}
