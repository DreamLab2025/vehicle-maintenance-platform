namespace Verendar.Location.Application.Services.Interfaces;

public interface IAdministrativeRegionService
{
    Task<ApiResponse<List<AdministrativeRegionResponse>>> GetAllAsync();
}
