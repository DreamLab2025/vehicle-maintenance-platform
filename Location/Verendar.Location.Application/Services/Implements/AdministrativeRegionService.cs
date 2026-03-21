namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Domain.Repositories.Interfaces;

public class AdministrativeRegionService(ILogger<AdministrativeRegionService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IAdministrativeRegionService
{
    public async Task<ApiResponse<List<AdministrativeRegionResponse>>> GetAllAsync()
    {
        try
        {
            const string cacheKey = "location:administrative-regions";
            var cached = await cacheService.GetAsync<List<AdministrativeRegionResponse>>(cacheKey);
            if (cached != null)
                return ApiResponse<List<AdministrativeRegionResponse>>.SuccessResponse(cached, "Lấy danh sách vùng miền thành công");

            var regions = await unitOfWork.AdministrativeRegions.GetAllAsync();
            var response = regions.Select(r => r.ToResponse()).ToList();

            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return ApiResponse<List<AdministrativeRegionResponse>>.SuccessResponse(response, "Lấy danh sách vùng miền thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting administrative regions");
            return ApiResponse<List<AdministrativeRegionResponse>>.FailureResponse("Lỗi khi lấy danh sách vùng miền");
        }
    }
}
