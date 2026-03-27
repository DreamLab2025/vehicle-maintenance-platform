using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Clients;

public interface IVehicleGarageClient
{
    Task<BookingVehicleSummary?> GetUserVehicleForBookingAsync(
        Guid ownerUserId,
        Guid userVehicleId,
        CancellationToken ct = default);
}
