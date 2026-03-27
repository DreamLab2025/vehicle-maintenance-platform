namespace Verendar.Garage.Application.Clients;

public interface IIdentityClient
{
    Task<Guid?> CreateMechanicUserAsync(CreateMemberUserRequest request, CancellationToken ct = default);
    Task<Guid?> CreateManagerUserAsync(CreateMemberUserRequest request, CancellationToken ct = default);
    Task<bool> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<bool> RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<bool> BulkDeactivateAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
}
