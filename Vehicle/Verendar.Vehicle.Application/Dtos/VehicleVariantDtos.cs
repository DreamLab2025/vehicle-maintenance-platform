    namespace Verendar.Vehicle.Application.Dtos
{
    public class VariantRequest
    {
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class VariantResponse
    {
        public Guid Id { get; set; }
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserVariantResponse : VariantResponse
    {
        public ModelResponse Model { get; set; } = null!;
    }

    public class VariantUpdateRequest
    {
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }
}
