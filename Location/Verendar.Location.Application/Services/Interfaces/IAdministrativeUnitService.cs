namespace Verendar.Location.Application.Services.Interfaces;

public interface IAdministrativeUnitService
{
    Task<ApiResponse<List<AdministrativeUnitResponse>>> GetAllAsync();
}
