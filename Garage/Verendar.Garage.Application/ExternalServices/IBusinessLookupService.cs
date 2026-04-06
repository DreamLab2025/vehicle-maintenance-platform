namespace Verendar.Garage.Application.ExternalServices;

public interface IBusinessLookupService
{
    Task<BusinessInfoDto?> LookupBusinessAsync(string taxCode, CancellationToken ct = default);
}
