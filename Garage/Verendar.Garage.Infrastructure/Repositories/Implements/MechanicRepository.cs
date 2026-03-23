namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageMemberRepository(GarageDbContext context)
    : PostgresRepository<GarageMember>(context), IGarageMemberRepository
{
    public Task<bool> IsManagerOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default) =>
        context.GarageMembers.AnyAsync(
            m => m.GarageBranchId == branchId
                 && m.UserId == userId
                 && m.Role == MemberRole.Manager
                 && m.DeletedAt == null,
            ct);

    public Task<bool> IsMemberOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default) =>
        context.GarageMembers.AnyAsync(
            m => m.GarageBranchId == branchId
                 && m.UserId == userId
                 && m.DeletedAt == null,
            ct);
}
