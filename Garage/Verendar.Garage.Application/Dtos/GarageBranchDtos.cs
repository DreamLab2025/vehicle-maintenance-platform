namespace Verendar.Garage.Application.Dtos;

// ── Nested DTOs ───────────────────────────────────────────────────────────────

public class AddressDto
{
    public string ProvinceCode { get; set; } = null!;
    public string WardCode { get; set; } = null!;
    public string? HouseNumber { get; set; }
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
    public WorkingHoursDto WorkingHours { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? TaxCode { get; set; }
    public BranchStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
