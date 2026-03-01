namespace Verendar.Vehicle.Application.Dtos
{
    public class BrandRequest
    {
        public Guid VehicleTypeId { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public string? Website { get; set; }

        public string? SupportPhone { get; set; }
    }

    public class BrandResponse
    {
        public Guid Id { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? SupportPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
