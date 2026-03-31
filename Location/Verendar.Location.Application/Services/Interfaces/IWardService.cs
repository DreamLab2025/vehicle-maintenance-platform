namespace Verendar.Location.Application.Services.Interfaces;

public interface IWardService
{
    Task<ApiResponse<WardResponse>> GetWardByCodeAsync(string code);
    Task<ApiResponse<WardBoundaryResponse>> GetWardBoundaryAsync(string code);
}
