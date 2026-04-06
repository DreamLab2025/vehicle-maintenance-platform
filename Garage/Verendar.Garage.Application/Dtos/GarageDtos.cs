using Verendar.Common.Shared;
using Verendar.Garage.Domain.Enums;

namespace Verendar.Garage.Application.Dtos;

public class GarageFilterRequest : PaginationRequest
{
    public GarageStatus? Status { get; set; }
}

public class UpdateGarageStatusRequest
{
    public GarageStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class UpdateBranchStatusRequest
{
    public BranchStatus Status { get; set; }
}

public class GarageRequest
{
    public string BusinessName { get; set; } = null!;
    public string? ShortName { get; set; }
    public string? TaxCode { get; set; }
    public string? LogoUrl { get; set; }
}

public class GarageResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortName { get; set; }
    public string? TaxCode { get; set; }
    public string? LogoUrl { get; set; }
    public GarageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GarageBranchSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public AddressDto Address { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public BranchStatus Status { get; set; }
}

public class GarageDetailResponse : GarageResponse
{
    public int BranchCount { get; set; }
    public List<GarageBranchSummaryResponse> Branches { get; set; } = [];
}

public class GarageMemberQueryRequest : PaginationRequest
{
    public Guid GarageId { get; set; }
    public Guid BranchId { get; set; }
    public string? Name { get; set; }
    public MemberRole? Role { get; set; }
    public MemberStatus? Status { get; set; }
}

public class AddMemberRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public MemberRole Role { get; set; }
    public Guid BranchId { get; set; }
}

public class UpdateMemberStatusRequest
{
    public MemberStatus Status { get; set; }
}

public class GarageMemberResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public Guid GarageId { get; set; }
    public MemberRole Role { get; set; }
    public MemberStatus Status { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
