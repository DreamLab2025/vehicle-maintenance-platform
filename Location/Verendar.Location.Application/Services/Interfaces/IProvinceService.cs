namespace Verendar.Location.Application.Services.Interfaces;

public interface IProvinceService
{
    Task<ApiResponse<List<ProvinceResponse>>> GetAllProvincesAsync();
    Task<ApiResponse<ProvinceResponse>> GetProvinceByCodeAsync(string code);
    Task<ApiResponse<List<WardResponse>>> GetWardsByProvinceAsync(string provinceCode);
}
