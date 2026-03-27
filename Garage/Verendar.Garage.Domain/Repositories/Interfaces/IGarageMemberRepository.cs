namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IGarageMemberRepository : IGenericRepository<GarageMember>
{
    Task<bool> IsManagerOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default);
    Task<bool> IsMemberOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default);
    Task<List<GarageMember>> GetActiveByGarageIdAsync(Guid garageId, CancellationToken ct = default);
}
