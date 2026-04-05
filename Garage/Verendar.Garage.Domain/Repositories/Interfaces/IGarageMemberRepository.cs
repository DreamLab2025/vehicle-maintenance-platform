namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageMemberRepository : IGenericRepository<GarageMember>
{
    Task<bool> IsManagerOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default);
    Task<bool> IsMemberOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default);
    Task<bool> IsActiveManagerOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default);

    Task<(Guid Id, string DisplayName)?> GetActiveMechanicForAssignmentAsync(
        Guid memberId,
        Guid branchId,
        CancellationToken ct = default);

    Task<List<GarageMember>> GetActiveByGarageIdAsync(Guid garageId, CancellationToken ct = default);

    Task<List<Guid>> GetActiveManagerUserIdsByBranchIdAsync(Guid branchId, CancellationToken ct = default);

    Task<GarageMember?> GetLatestActiveMembershipWithBranchAsync(Guid userId, CancellationToken ct = default);
}
