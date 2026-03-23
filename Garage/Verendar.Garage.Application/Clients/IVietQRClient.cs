using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Clients;

public interface IVietQRClient
{
    Task<BusinessInfoDto?> LookupBusinessAsync(string taxCode, CancellationToken ct = default);
}
