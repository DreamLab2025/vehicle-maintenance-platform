using Verendar.Common.Shared;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Dtos
{
    public class ModelRequest
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public Guid BrandId { get; set; }
        public Guid TypeId { get; set; }
        public int? ReleaseYear { get; set; }
        public VehicleFuelType? FuelType { get; set; }
        public VehicleTransmissionType? TransmissionType { get; set; }
        public List<ModelImageItem> Images { get; set; } = new();
        public int? EngineDisplacement { get; set; }
        public decimal? EngineCapacity { get; set; }
    }

    public class ModelImageItem
    {
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class ModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public Guid TypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public int? ReleaseYear { get; set; }
        public VehicleFuelType? FuelType { get; set; }
        public string FuelTypeName { get; set; } = null!;
        public VehicleTransmissionType? TransmissionType { get; set; }
        public string TransmissionTypeName { get; set; } = null!;
        public string? EngineDisplacementDisplay { get; set; }
        public decimal? EngineCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelResponseWithVariants : ModelResponse
    {
        public List<VehicleVariantResponse> Variants { get; set; } = new();
    }

    public class ModelFilterRequest : PaginationRequest
    {
        public Guid? TypeId { get; set; }
        public Guid? BrandId { get; set; }
        public string? ModelName { get; set; }
        public VehicleTransmissionType? TransmissionType { get; set; }
        public int? EngineDisplacement { get; set; }
        public int? ReleaseYear { get; set; }

        /// <summary>
        /// Chuẩn hóa pagination và trim search string (ModelName).
        /// </summary>
        public override void Normalize()
        {
            base.Normalize();
            if (!string.IsNullOrWhiteSpace(ModelName))
                ModelName = ModelName.Trim();
            else
                ModelName = null;
        }
    }
}
