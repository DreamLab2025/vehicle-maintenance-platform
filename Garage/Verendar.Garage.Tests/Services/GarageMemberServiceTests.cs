using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Dtos.Clients;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageMemberServiceTests
{
    [Fact]
    public async Task AddMemberAsync_WhenOwnerAddsManager_CreatesMemberAndReturns201()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var createdUserId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>();

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);
        m.Members.Setup(r => r.AddAsync(It.IsAny<GarageMember>()))
            .ReturnsAsync((GarageMember member) =>
            {
                member.Id = Guid.NewGuid();
                members.Add(member);
                return member;
            });

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        identity.Setup(i => i.CreateManagerUserAsync(It.IsAny<CreateMemberUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUserId);

        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.AddMemberAsync(garageId, ownerId, new AddMemberRequest
        {
            FullName = "Manager A",
            Email = "manager@test.dev",
            PhoneNumber = "0900000000",
            Role = MemberRole.Manager,
            BranchId = branchId
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Member.AddSuccess);
        result.Data!.Role.Should().Be(MemberRole.Manager);
        members.Should().ContainSingle(x => x.UserId == createdUserId && x.Role == MemberRole.Manager);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_WhenManagerAddsManager_Returns403()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var managerUserId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>
        {
            new() { Id = Guid.NewGuid(), UserId = managerUserId, GarageBranchId = branchId, Role = MemberRole.Manager, Status = MemberStatus.Active, DisplayName = "M", Email = "m@test.dev" }
        };

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.AddMemberAsync(garageId, managerUserId, new AddMemberRequest
        {
            FullName = "Manager B",
            Email = "manager2@test.dev",
            PhoneNumber = "0900000001",
            Role = MemberRole.Manager,
            BranchId = branchId
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Member.ManagerOnlyMechanic);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMembersAsync_WhenUnauthorizedUser_Returns403()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>();

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);
        m.Members.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageMember, bool>>>(),
                It.IsAny<Func<IQueryable<GarageMember>, IOrderedQueryable<GarageMember>>>()))
            .Throws(new InvalidOperationException("Should not be called"));

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.GetMembersAsync(new GarageMemberQueryRequest { GarageId = garageId, BranchId = branchId }, requesterId, CancellationToken.None);

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Member.ForbiddenListMembers);
    }

    [Fact]
    public async Task UpdateMemberStatusAsync_WhenManagerUpdatesManager_Returns403()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var managerUserId = Guid.NewGuid();
        var targetManagerId = Guid.NewGuid();

        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>
        {
            new() { Id = Guid.NewGuid(), UserId = managerUserId, GarageBranchId = branchId, Role = MemberRole.Manager, Status = MemberStatus.Active, DisplayName = "Requester", Email = "rq@test.dev" },
            new() { Id = targetManagerId, UserId = Guid.NewGuid(), GarageBranchId = branchId, Role = MemberRole.Manager, Status = MemberStatus.Active, DisplayName = "Target", Email = "target@test.dev" }
        };

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.UpdateMemberStatusAsync(targetManagerId, managerUserId, new UpdateMemberStatusRequest { Status = MemberStatus.Inactive });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Member.ManagerOnlyUpdateMechanic);
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenOwnerDeletesMember_SetsDeletedAtAndReturns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>
        {
            new() { Id = memberId, UserId = Guid.NewGuid(), GarageBranchId = branchId, Role = MemberRole.Mechanic, Status = MemberStatus.Active, DisplayName = "Mechanic", Email = "m@test.dev" }
        };

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.RemoveMemberAsync(memberId, ownerId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Member.RemoveSuccess);
        members.Single(x => x.Id == memberId).DeletedAt.Should().NotBeNull();
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_WhenIdentityReturnsNull_Returns400()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>();

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);

        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        identity.Setup(i => i.CreateMechanicUserAsync(It.IsAny<CreateMemberUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.AddMemberAsync(garageId, ownerId, new AddMemberRequest
        {
            FullName = "Mechanic A",
            Email = "mechanic@test.dev",
            PhoneNumber = "0901000000",
            Role = MemberRole.Mechanic,
            BranchId = branchId
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Member.IdentityCreateFailed);
    }

    [Fact]
    public async Task GetMembersAsync_WhenOwnerRequests_ReturnsPagedMembers()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), GarageBranchId = branchId, Role = MemberRole.Mechanic, Status = MemberStatus.Active, DisplayName = "M1", Email = "m1@test.dev" }
        };

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);
        m.Members.Setup(r => r.GetPagedByBranchIdAsync(
                branchId,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<MemberRole?>(),
                It.IsAny<MemberStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((members, members.Count));
        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.GetMembersAsync(new GarageMemberQueryRequest { GarageId = garageId, BranchId = branchId }, ownerId, CancellationToken.None);

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, EndpointMessages.Member.ListSuccess, 1, 1);
    }

    [Fact]
    public async Task UpdateMemberStatusAsync_WhenOwnerUpdatesMechanic_Returns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" } };
        var branches = new List<GarageBranch> { new() { Id = branchId, GarageId = garageId, Name = "B", Slug = "b", Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" }, WorkingHours = new() { Schedule = [] } } };
        var members = new List<GarageMember>
        {
            new() { Id = memberId, UserId = Guid.NewGuid(), GarageBranchId = branchId, Role = MemberRole.Mechanic, Status = MemberStatus.Active, DisplayName = "Mechanic", Email = "m@test.dev" }
        };

        var m = new GarageUnitOfWorkMock();
        SetupBasicRepositories(m, garages, branches, members);
        var identity = new Mock<IIdentityClient>(MockBehavior.Strict);
        var sut = new GarageMemberService(NullLogger<GarageMemberService>.Instance, m.UnitOfWork.Object, identity.Object);

        var result = await sut.UpdateMemberStatusAsync(memberId, ownerId, new UpdateMemberStatusRequest { Status = MemberStatus.Inactive });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Member.UpdateStatusSuccess);
        members[0].Status.Should().Be(MemberStatus.Inactive);
    }

    private static void SetupBasicRepositories(
        GarageUnitOfWorkMock mock,
        List<GarageEntity> garages,
        List<GarageBranch> branches,
        List<GarageMember> members)
    {
        mock.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));

        mock.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageBranch, bool>> expr) =>
                branches.FirstOrDefault(b => expr.Compile()(b)));

        mock.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageMember, bool>> expr) =>
                members.FirstOrDefault(m => expr.Compile()(m)));

        mock.Members.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageMember, bool>>>(),
                It.IsAny<Func<IQueryable<GarageMember>, IOrderedQueryable<GarageMember>>>()))
            .ReturnsAsync((int pageNumber, int pageSize, Expression<Func<GarageMember, bool>> filter, Func<IQueryable<GarageMember>, IOrderedQueryable<GarageMember>> orderBy) =>
            {
                var query = members.AsQueryable().Where(filter);
                var ordered = orderBy(query);
                var paged = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return (paged, query.Count());
            });
    }
}
