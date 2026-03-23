using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageService
{
    Task<ApiResponse<GarageResponse>> CreateGarageAsync(Guid ownerId, GarageRequest request);
}
