namespace Verendar.Garage.Application.Clients;

public interface IIdentityClient
{
    Task<Guid?> CreateMechanicUserAsync(CreateMemberUserRequest request, CancellationToken ct = default);
    Task<Guid?> CreateManagerUserAsync(CreateMemberUserRequest request, CancellationToken ct = default);
}
