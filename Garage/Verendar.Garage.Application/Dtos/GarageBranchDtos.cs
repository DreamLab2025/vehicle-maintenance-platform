namespace Verendar.Garage.Application.Dtos;

// ── Nested DTOs ───────────────────────────────────────────────────────────────

public class AddressDto
{
    public string ProvinceCode { get; set; } = null!;
    public string WardCode { get; set; } = null!;
    public string StreetDetail { get; set; } = null!;
}

public class DayScheduleDto
{
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class WorkingHoursDto
{
    public Dictionary<DayOfWeek, DayScheduleDto> Schedule { get; set; } = [];
}

// ── Request ───────────────────────────────────────────────────────────────────

public class GarageBranchRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TaxCode { get; set; }
    public AddressDto Address { get; set; } = null!;
    public WorkingHoursDto WorkingHours { get; set; } = null!;
}

// ── Map Search ────────────────────────────────────────────────────────────────

public class BranchMapSearchRequest : PaginationRequest
{
    public string? Address { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public double RadiusKm { get; set; } = 10;
}

public class GarageInfoDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public GarageStatus Status { get; set; }
}

public class BranchMapItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public AddressDto Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public MapLinksDto? MapLinks { get; set; }
    public string? PhoneNumber { get; set; }
    public BranchStatus Status { get; set; }
    public GarageInfoDto Garage { get; set; } = null!;
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

// ── Map Links ─────────────────────────────────────────────────────────────────

public class MapLinksDto
{
    public string GoogleMaps { get; set; } = null!;
    public string AppleMaps { get; set; } = null!;
    public string Waze { get; set; } = null!;
    public string OpenStreetMap { get; set; } = null!;
}

// ── Response ──────────────────────────────────────────────────────────────────

public class GarageBranchResponse
{
    public Guid Id { get; set; }
    public Guid GarageId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public AddressDto Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public MapLinksDto? MapLinks { get; set; }
    public WorkingHoursDto WorkingHours { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? TaxCode { get; set; }
    public BranchStatus Status { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
