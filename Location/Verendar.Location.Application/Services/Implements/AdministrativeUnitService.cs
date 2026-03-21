namespace Verendar.Location.Application.Services.Implements;

using Verendar.Common.Caching;
using Verendar.Location.Application.Mappings;
using Verendar.Location.Application.Services.Interfaces;
using Verendar.Location.Domain.Repositories.Interfaces;

public class AdministrativeUnitService(ILogger<AdministrativeUnitService> logger, IUnitOfWork unitOfWork, ICacheService cacheService) : IAdministrativeUnitService
{
    public async Task<ApiResponse<List<AdministrativeUnitResponse>>> GetAllAsync()
    {
        try
        {
            const string cacheKey = "location:administrative-units";
            var cached = await cacheService.GetAsync<List<AdministrativeUnitResponse>>(cacheKey);
            if (cached != null)
                return ApiResponse<List<AdministrativeUnitResponse>>.SuccessResponse(cached, "Lấy danh sách loại đơn vị thành công");

            var units = await unitOfWork.AdministrativeUnits.GetAllAsync();
            var response = units.Select(u => u.ToResponse()).ToList();

            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return ApiResponse<List<AdministrativeUnitResponse>>.SuccessResponse(response, "Lấy danh sách loại đơn vị thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting administrative units");
            return ApiResponse<List<AdministrativeUnitResponse>>.FailureResponse("Lỗi khi lấy danh sách loại đơn vị");
        }
    }
}
