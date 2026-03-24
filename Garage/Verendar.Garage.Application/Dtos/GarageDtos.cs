using Verendar.Common.Shared;
using Verendar.Garage.Domain.Enums;

namespace Verendar.Garage.Application.Dtos;

public class GarageFilterRequest : PaginationRequest
{
    public GarageStatus? Status { get; set; }
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
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public BranchStatus Status { get; set; }
}

public class GarageDetailResponse : GarageResponse
{
    public int BranchCount { get; set; }
    public List<GarageBranchSummaryResponse> Branches { get; set; } = [];
}
