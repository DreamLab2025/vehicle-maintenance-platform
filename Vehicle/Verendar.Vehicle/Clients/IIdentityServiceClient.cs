namespace Verendar.Vehicle.Clients;

public interface IIdentityServiceClient
{
    Task<string?> GetUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
