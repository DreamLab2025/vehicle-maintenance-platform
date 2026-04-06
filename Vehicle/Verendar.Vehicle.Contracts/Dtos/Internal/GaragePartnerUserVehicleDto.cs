namespace Verendar.Vehicle.Contracts.Dtos.Internal;

public record GaragePartnerUserVehicleDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? LicensePlate { get; init; }
    public string? Vin { get; init; }
    public int CurrentOdometer { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string VariantColor { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}
