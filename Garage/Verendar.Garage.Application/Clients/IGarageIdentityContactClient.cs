using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Clients;

public interface IGarageIdentityContactClient
{
    Task<BookingCustomerSummary?> GetCustomerContactAsync(Guid userId, CancellationToken ct = default);
}
