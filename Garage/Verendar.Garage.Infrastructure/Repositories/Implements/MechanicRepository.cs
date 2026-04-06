using Microsoft.EntityFrameworkCore;

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

    public Task<bool> IsActiveManagerOfBranchAsync(Guid branchId, Guid userId, CancellationToken ct = default) =>
        context.GarageMembers.AnyAsync(
            m => m.GarageBranchId == branchId
                 && m.UserId == userId
                 && m.Role == MemberRole.Manager
                 && m.Status == MemberStatus.Active
                 && m.DeletedAt == null,
            ct);

    public async Task<(Guid Id, string DisplayName)?> GetActiveMechanicForAssignmentAsync(
        Guid memberId,
        Guid branchId,
        CancellationToken ct = default)
    {
        var row = await context.GarageMembers
            .AsNoTracking()
            .Where(m =>
                m.Id == memberId
                && m.GarageBranchId == branchId
                && m.Role == MemberRole.Mechanic
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null)
            .Select(m => new { m.Id, m.DisplayName })
            .FirstOrDefaultAsync(ct);

        return row is null ? null : (row.Id, row.DisplayName);
    }

    public Task<List<GarageMember>> GetActiveByGarageIdAsync(Guid garageId, CancellationToken ct = default) =>
        context.GarageMembers
            .Where(m => m.GarageBranch.GarageId == garageId
                        && m.Status == MemberStatus.Active
                        && m.DeletedAt == null)
            .ToListAsync(ct);

    public Task<List<Guid>> GetActiveManagerUserIdsByBranchIdAsync(Guid branchId, CancellationToken ct = default) =>
        context.GarageMembers
            .AsNoTracking()
            .Where(m => m.GarageBranchId == branchId
                        && m.Role == MemberRole.Manager
                        && m.Status == MemberStatus.Active
                        && m.DeletedAt == null)
            .Select(m => m.UserId)
            .ToListAsync(ct);

    public Task<GarageMember?> GetLatestActiveMembershipWithBranchAsync(Guid userId, CancellationToken ct = default) =>
        context.GarageMembers
            .AsNoTracking()
            .Include(m => m.GarageBranch)
            .ThenInclude(b => b.Garage)
            .Where(m => m.UserId == userId
                        && m.DeletedAt == null
                        && m.Status == MemberStatus.Active)
            .Where(m => m.GarageBranch.DeletedAt == null
                        && m.GarageBranch.Garage.DeletedAt == null)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<(List<GarageMember> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        int pageNumber,
        int pageSize,
        string? name = null,
        MemberRole? role = null,
        MemberStatus? status = null,
        CancellationToken ct = default)
    {
        var query = context.GarageMembers
            .AsNoTracking()
            .Where(m => m.GarageBranchId == branchId && m.DeletedAt == null);

        if (name is not null)
            query = query.Where(m => EF.Functions.ILike(m.DisplayName, $"%{name}%"));
        if (role.HasValue)
            query = query.Where(m => m.Role == role.Value);
        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
