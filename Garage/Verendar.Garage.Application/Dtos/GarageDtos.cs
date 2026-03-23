namespace Verendar.Garage.Application.Dtos;

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
