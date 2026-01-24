namespace Verendar.Vehicle.Application.Dtos
{
    public class VehicleVariantRequest
    {
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class VehicleVariantResponse
    {
        public Guid Id { get; set; }
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserVehicleVariantResponse : VehicleVariantResponse
    {
        public ModelResponse Model { get; set; } = null!;
    }

    public class VehicleVariantUpdateRequest
    {
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }
}
