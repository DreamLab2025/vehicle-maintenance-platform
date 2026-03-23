using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Dtos.Clients;

namespace Verendar.Garage.Application.Clients;

public interface IVietQRClient
{
    Task<BusinessInfoDto?> LookupBusinessAsync(string taxCode, CancellationToken ct = default);
}
