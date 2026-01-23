using System.ComponentModel.DataAnnotations;
using Verendar.Common.Shared;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Dtos
{
    public class ModelRequest
    {
        [Required(ErrorMessage = "Tên mẫu xe không được để trống")]
        [MaxLength(150, ErrorMessage = "Tên mẫu xe không được vượt quá 150 ký tự")]
        public string Name { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "Mã mẫu xe không được vượt quá 100 ký tự")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Loại xe không được để trống")]
        public Guid TypeId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
        public int? ReleaseYear { get; set; }

        public VehicleFuelType? FuelType { get; set; }

        public VehicleTransmissionType? TransmissionType { get; set; }

        public List<ModelImageItem> Images { get; set; } = new();

        [Range(1, 10000, ErrorMessage = "Phân khối phải từ 1 đến 10000 cc")]
        public int? EngineDisplacement { get; set; }

        [Range(0.1, 20, ErrorMessage = "Dung tích động cơ phải từ 0.1 đến 20 lít")]
        public decimal? EngineCapacity { get; set; }
    }

    public class ModelImageItem
    {
        [Required(ErrorMessage = "Màu xe không được để trống")]
        [MaxLength(50, ErrorMessage = "Màu xe không được vượt quá 50 ký tự")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "Mã màu không được để trống")]
        [MaxLength(7, ErrorMessage = "Mã màu không được vượt quá 7 ký tự")]
        public string HexCode { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ảnh không được để trống")]
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string ImageUrl { get; set; } = null!;
    }

    public class BulkModelItem
    {
        [Required(ErrorMessage = "Tên mẫu xe không được để trống")]
        [MaxLength(150, ErrorMessage = "Tên mẫu xe không được vượt quá 150 ký tự")]
        public string Name { get; set; } = null!;

        [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
        public int? ReleaseYear { get; set; }

        public VehicleFuelType? FuelType { get; set; }

        public VehicleTransmissionType? TransmissionType { get; set; }

        public List<ModelImageItem> Images { get; set; } = new();

        [Range(1, 10000, ErrorMessage = "Phân khối phải từ 1 đến 10000 cc")]
        public int? EngineDisplacement { get; set; }

        [Range(0.1, 20, ErrorMessage = "Dung tích động cơ phải từ 0.1 đến 20 lít")]
        public decimal? EngineCapacity { get; set; }
    }

    public class BulkModelRequest
    {
        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Loại xe không được để trống")]
        public Guid TypeId { get; set; }

        [Required(ErrorMessage = "Danh sách mẫu xe không được để trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 mẫu xe")]
        public List<BulkModelItem> Models { get; set; } = new();
    }

    public class BulkModelFileRequest
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự")]
        public string BrandName { get; set; } = null!;

        [Required(ErrorMessage = "Tên loại xe không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên loại xe không được vượt quá 100 ký tự")]
        public string TypeName { get; set; } = null!;

        [Required(ErrorMessage = "Danh sách mẫu xe không được để trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 mẫu xe")]
        public List<BulkModelItem> Models { get; set; } = new();
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
        public string? EngineDisplacementDisplay { get; set; } // Hiển thị phân khối với đơn vị "cc"
        public decimal? EngineCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelResponseWithVariants : ModelResponse
    {
        public List<VehicleVariantResponse> Variants { get; set; } = new();
    }

    public class BulkModelResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<ModelResponse> SuccessfulModels { get; set; } = new();
        public List<BulkOperationError> Errors { get; set; } = new();
    }

    public class ModelFilterRequest : PaginationRequest
    {
        public Guid? TypeId { get; set; }
        public Guid? BrandId { get; set; }
        public string? ModelName { get; set; }
        public string? Color { get; set; }
        public VehicleTransmissionType? TransmissionType { get; set; }
        public int? EngineDisplacement { get; set; }
        public int? ReleaseYear { get; set; }
    }
}
